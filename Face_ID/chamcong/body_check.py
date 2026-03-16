import cv2
import numpy as np
import torch
import torchreid
from ultralytics import YOLO
from chamcong import camera_state
import time
from PIL import Image
from chamcong.ketnoi_SQL import get_ketnoi
from discord_notify import send_unknown_alert

# ================= KẾT NỐI DB =================
conn = get_ketnoi()
BODY_DB = []

# ================= DISCORD COOLDOWN =================
last_alert_time = 0
ALERT_COOLDOWN = 5   # 10 giây gửi 1 lần


def alert_once(frame, msg):
    global last_alert_time

    now = time.time()
    if now - last_alert_time > ALERT_COOLDOWN:

        print(msg)
        send_unknown_alert(frame, msg)

        last_alert_time = now
# ================= LOAD BODY EMBEDDING =================
def reload_body_db():
    BODY_DB.clear()

    cur = conn.cursor()
    cur.execute("SELECT EmployeeId, Embedding FROM BodyEmbeddings")

    rows = cur.fetchall()

    for emp_id, emb in rows:
        BODY_DB.append(
            (emp_id, np.frombuffer(emb, np.float32))
        )

    print(f"✅ Loaded {len(BODY_DB)} body embeddings")


# ================= COSINE SIMILARITY =================
def cosine_similarity(a, b):
    return float(
        np.dot(a, b) /
        (np.linalg.norm(a) * np.linalg.norm(b) + 1e-8)
    )


# ================= LOAD MODEL =================
print("🚀 Loading BODY models...")

detector = YOLO("yolov8n.pt")

model = torchreid.models.build_model(
    name="osnet_x1_0",
    num_classes=1000,
    pretrained=True
)
model.eval()

transform = torchreid.data.transforms.build_transforms(
    height=256,
    width=128
)[1]

print("✅ BODY models ready")


# ======================================================
def verify_body(frame):
    """
    BODY CHECK:

    ✔ DB có người -> so sánh embedding
    ✔ DB trống -> coi tất cả là UNKNOWN
    ✔ UNKNOWN -> gửi Discord
    """

    # ===== LOAD DB =====
    if not BODY_DB:
        reload_body_db()

    db_empty = len(BODY_DB) == 0

    if db_empty:
        print("⚠️ BODY DB EMPTY → ALL UNKNOWN")

    with camera_state.state_lock:
        camera_state.body_verified = False

    # ===== DETECT PERSON =====
    results = detector(frame, verbose=False)

    found_person = False

    for r in results:
        for box in r.boxes:

            # YOLO class 0 = person
            if int(box.cls[0]) != 0:
                continue

            found_person = True

            x1, y1, x2, y2 = box.xyxy[0].cpu().numpy().astype(int)
            crop = frame[y1:y2, x1:x2]

            if crop.size == 0:
                continue

            # ===== PREPROCESS =====
            # ===== PREPROCESS =====
            img = cv2.resize(crop, (128, 256))

            # OpenCV BGR -> RGB
            img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

            # numpy -> PIL
            img = Image.fromarray(img)

            # transform -> tensor
            img = transform(img).unsqueeze(0)



            # ===== EXTRACT FEATURE =====
            with torch.no_grad():
                feat = model(img)

            emb = feat.squeeze().numpy()

            best_id = None
            best_score = 0.0

            # ===== SO SÁNH (nếu DB có dữ liệu) =====
            if not db_empty:
                for emp_id, db_emb in BODY_DB:
                    sim = cosine_similarity(emb, db_emb)

                    if sim > best_score:
                        best_score = sim
                        best_id = emp_id

            BODY_THRESHOLD = 0.6

            # ===== DECISION =====
            if (not db_empty) and best_score >= BODY_THRESHOLD:
                # ✅ Nhân viên hợp lệ
                camera_state.body_verified = True
                label = f"Employee {best_id} ({best_score:.2f})"
                color = (0, 255, 0)

            else:
                # 🚨 UNKNOWN (bao gồm DB trống)
                alert_once(crop, f"🚨 UNKNOWN BODY ({best_score:.2f})")

                label = f"UNKNOWN ({best_score:.2f})"
                color = (0, 0, 255)

            # ===== DRAW =====
            cv2.rectangle(frame, (x1, y1), (x2, y2), color, 2)

            cv2.putText(
                frame,
                label,
                (x1, y1 - 10),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.7,
                color,
                2
            )

    # Nếu KHÔNG thấy người → không alert
    if not found_person:
        pass

    return frame