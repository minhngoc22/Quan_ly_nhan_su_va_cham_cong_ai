import time
import numpy as np
import cv2
import torch
import torchreid
from ultralytics import YOLO
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
# LOAD MODEL
# ===============================
print("🔄 Loading YOLO...")
detector = YOLO("yolov8n.pt")

print("🔄 Loading ReID model...")
model = torchreid.models.build_model(
    name='osnet_x1_0',
    num_classes=1000,
    pretrained=True
)
model.eval()

transform = torchreid.data.transforms.build_transforms(
    height=256,
    width=128
)[1]

# ===============================
# STATE
# ===============================
REQUIRED_FRAMES = 6
correct_frames = 0
embeddings = []
current_employee = None

# ===============================
# CHECK BODY VALID
# ===============================
def body_valid(box, frame_shape):
    h, w, _ = frame_shape
    x1, y1, x2, y2 = box

    body_height = y2 - y1

    # Body phải chiếm ít nhất 60% chiều cao frame
    return body_height > h * 0.6

# ===============================
# ENROLL STEP
# ===============================
def enroll_body_step(frame, employee_code):
    global correct_frames, embeddings, current_employee

    if current_employee != employee_code:
        current_employee = employee_code
        correct_frames = 0
        embeddings = []

    results = detector(frame, verbose=False)

    frame = put_text_vi(frame, "THU DÁNG NGƯỜI", (20,30), 32, (255,255,0))
    frame = put_text_vi(frame, "Đứng thẳng – thấy toàn thân", (20,70), 30, (0,255,0))

    persons = []

    for r in results:
        for box in r.boxes:
            if int(box.cls[0]) == 0:
                persons.append(box.xyxy[0].cpu().numpy().astype(int))

    if len(persons) != 1:
        frame = put_text_vi(frame, "Chỉ 1 người trước camera", (20,120), 28, (255,0,0))
        return frame, False

    x1, y1, x2, y2 = persons[0]
    cv2.rectangle(frame, (x1,y1),(x2,y2),(0,255,0),2)

    if not body_valid((x1,y1,x2,y2), frame.shape):
        frame = put_text_vi(frame, "Lùi xa camera hơn", (20,120), 28, (255,0,0))
        correct_frames = 0
        return frame, False

    correct_frames += 1
    frame = put_text_vi(
        frame,
        f"Ổn định ({correct_frames}/{REQUIRED_FRAMES})",
        (20,160),
        28,
        (0,255,0)
    )

    if correct_frames >= REQUIRED_FRAMES:

        body = frame[y1:y2, x1:x2]

        img = cv2.resize(body, (128, 256))

        # convert BGR -> RGB
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

        # convert numpy -> PIL
        img = Image.fromarray(img)

        img = transform(img).unsqueeze(0)

        with torch.no_grad():
            features = model(img)

        embeddings.append(features.squeeze().numpy())

        correct_frames = 0
        time.sleep(0.3)

        if len(embeddings) >= 5:
            return frame, True

    return frame, False

# ===============================
# SAVE BODY EMBEDDING
# ===============================
def save_body_embedding(employee_code):
    global embeddings

    if len(embeddings) == 0:
        print("❌ Chưa có embedding")
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
        INSERT INTO BodyEmbeddings (EmployeeId, Embedding)
        VALUES (?, ?)
    """, employee_id, mean_emb.tobytes())

    conn.commit()
    conn.close()

    embeddings.clear()
    print("✅ ĐÃ LƯU BODY ID")
