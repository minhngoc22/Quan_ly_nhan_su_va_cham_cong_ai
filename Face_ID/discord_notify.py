import requests
import time
import cv2

WEBHOOK_URL = "https://discord.com/api/webhooks/1471870860943036668/AwprkOFjlas7O8Hli4dDvGi3IwvfYc0Rch2M8BC-KRQOmRom_0j-C4eAhhbVxH99OYkn"

last_alert_time = 0
COOLDOWN = 5  # chống spam


def send_unknown_alert(frame=None, message="🚨 Phát hiện người lạ"):
    global last_alert_time

    now = time.time()
    if now - last_alert_time < COOLDOWN:
        return

    last_alert_time = now

    data = {
        "content": message
    }

    try:
        # ===== có frame -> gửi ảnh trực tiếp =====
        if frame is not None:

            # convert frame -> jpg memory
            _, buffer = cv2.imencode(".jpg", frame)

            files = {
                "file": ("alert.jpg", buffer.tobytes(), "image/jpeg")
            }

            requests.post(WEBHOOK_URL, data=data, files=files, timeout=5)

        else:
            requests.post(WEBHOOK_URL, json=data, timeout=5)

        print("📨 Discord alert sent:", message)

    except Exception as e:
        print("Discord Error:", e)