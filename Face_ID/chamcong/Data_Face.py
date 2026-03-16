import insightface
import time
import numpy as np
import cv2
from PIL import ImageFont, ImageDraw, Image
from chamcong.ketnoi_SQL import get_ketnoi

# ===============================
# FONT
# ===============================
FONT_CACHE = {
    28: ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 28),
    30: ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 30),
    32: ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 32),
}

def put_text_vi(frame, text, position, size=30, color=(0,255,0)):
    img = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    draw = ImageDraw.Draw(img)
    draw.text(position, text, font=FONT_CACHE[size], fill=color)
    return cv2.cvtColor(np.array(img), cv2.COLOR_RGB2BGR)

# ===============================
# 4 HƯỚNG
# ===============================
DIRECTIONS = [
    ("up",    "Ngẩng đầu lên"),
    ("down",  "Cúi đầu xuống"),
    ("left",  "Quay mặt sang TRÁI"),
    ("right", "Quay mặt sang PHẢI"),
]

# ===============================
# MODEL
# ===============================
app = insightface.app.FaceAnalysis(
    name="buffalo_s",
    providers=["CPUExecutionProvider"]
)
app.prepare(ctx_id=0, det_size=(512, 512))

# ===============================
# STATE
# ===============================
current_step = 0
correct_frames = 0
REQUIRED_FRAMES = 6
embeddings = []
current_employee = None

# ===============================
# CHECK DIRECTION
# ===============================
def check_direction(face, direction):
    yaw, pitch, _ = np.degrees(face.pose)

    if direction == "left":
        return yaw > 8
    if direction == "right":
        return yaw < -8
    if direction == "up":
        return pitch < -12
    if direction == "down":
        return pitch > 12
    return False

# ===============================
# ENROLL – XỬ LÝ 1 FRAME
# ===============================
def enroll_face_step(frame, employee_code):
    """
    Trả về:
    - frame đã vẽ UI
    - done = True nếu hoàn tất 4 hướng
    """
    global current_step, correct_frames, embeddings, current_employee

    if current_employee != employee_code:
        current_employee = employee_code
        current_step = 0
        correct_frames = 0
        embeddings = []

    faces = app.get(frame)

    step_key, guide_text = DIRECTIONS[current_step]

    frame = put_text_vi(frame, f"Bước {current_step+1}/4", (20, 30), 30, (255,255,0))
    frame = put_text_vi(frame, guide_text, (20, 70), 32, (0,255,0))

    if len(faces) != 1:
        frame = put_text_vi(frame, "Chỉ 1 người trước camera", (20, 120), 28, (255,0,0))
        return frame, False

    face = faces[0]
    box = face.bbox.astype(int)
    cv2.rectangle(frame, (box[0], box[1]), (box[2], box[3]), (0,255,0), 2)

    if check_direction(face, step_key):
        correct_frames += 1
        frame = put_text_vi(
            frame,
            f"ĐÚNG ({correct_frames}/{REQUIRED_FRAMES})",
            (20, 160),
            28,
            (0,255,0)
        )

        if correct_frames >= REQUIRED_FRAMES:
            embeddings.append(face.embedding)
            current_step += 1
            correct_frames = 0
            time.sleep(0.4)

            if current_step == 4:
                return frame, True
    else:
        correct_frames = 0
        frame = put_text_vi(frame, "CHƯA ĐÚNG HƯỚNG", (20,160), 28, (255,0,0))

    return frame, False

# ===============================
# SAVE EMBEDDING
# ===============================
def save_face_embedding(employee_code):
    global embeddings

    if len(embeddings) != 4:
        print("❌ Chưa đủ 4 hướng")
        return

    mean_emb = np.mean(embeddings, axis=0)

    conn = get_ketnoi()
    cursor = conn.cursor()

    cursor.execute(
        "SELECT Id FROM Employees WHERE EmployeeCode = ?",
        employee_code
    )
    row = cursor.fetchone()

    if not row:
        print("❌ Không tìm thấy nhân viên")
        return

    employee_id = row[0]

    cursor.execute("""
        INSERT INTO FaceEmbeddings (EmployeeId, Embedding)
        VALUES (?, ?)
    """, employee_id, mean_emb.tobytes())

    conn.commit()
    conn.close()

    embeddings.clear()
    print("✅ ĐÃ LƯU FACE ID – 4 HƯỚNG")
