from fastapi import FastAPI
import threading
from chamcong import camera_state
from camera_loop import camera_loop

app = FastAPI()

camera_thread = None


# =====================================================
# HELPER START CAMERA
# =====================================================
def start_camera(mode, employee=None):
    global camera_thread

    with camera_state.state_lock:

        if camera_state.camera_running:
            return {
                "success": False,
                "msg": "Camera đang chạy"
            }

        # reset state
        camera_state.stop_camera = False
        camera_state.current_mode = mode
        camera_state.target_employee = employee
        camera_state.camera_running = True

        # reset runtime state
        camera_state.last_face_employee = None
        camera_state.last_face_time = 0
        camera_state.body_verified = False
        camera_state.body_score = 0
        camera_state.face_detected = False
        camera_state.body_detected = False

    camera_thread = threading.Thread(
        target=camera_loop,
        daemon=True
    )
    camera_thread.start()

    return {
        "success": True,
        "msg": f"Camera started ({mode})"
    }


# =====================================================
# CHECKIN (FACE MODE)
# =====================================================
@app.post("/checkin")
def start_checkin():
    return start_camera("checkin")


# =====================================================
# REGISTER FACE
# =====================================================
@app.post("/register_face/{employee_code}")
def register_face(employee_code: str):

    res = start_camera("register_face", employee_code)

    if res["success"]:
        res["employee"] = employee_code
        res["msg"] = "Bắt đầu đăng ký khuôn mặt"

    return res


# =====================================================
# REGISTER BODY
# =====================================================
@app.post("/register_body/{employee_code}")
def register_body(employee_code: str):

    res = start_camera("register_body", employee_code)

    if res["success"]:
        res["employee"] = employee_code
        res["msg"] = "Bắt đầu đăng ký dáng người"

    return res


# =====================================================
# CHECK BODY ONLY
# =====================================================
@app.post("/check-body")
def start_check_body():
    return start_camera("check_body")


# =====================================================
# REALTIME FACE STATUS
# =====================================================
@app.get("/check-face")
def check_face():

    with camera_state.state_lock:
        return {
            "success": True,
            "employee": camera_state.last_face_employee,
            "faceTime": camera_state.last_face_time,
            "faceDetected": camera_state.face_detected
        }


# =====================================================
# REALTIME BODY STATUS
# =====================================================
@app.get("/check-body-status")
def check_body_status():

    with camera_state.state_lock:
        return {
            "success": True,
            "bodyVerified": camera_state.body_verified,
            "bodyScore": camera_state.body_score,
            "bodyDetected": camera_state.body_detected
        }


# =====================================================
# 🔥 FULL STATUS (FACE + BODY)
# =====================================================
@app.get("/check-status")
def check_status():

    with camera_state.state_lock:

        face_ok = camera_state.last_face_employee is not None
        body_ok = camera_state.body_verified

        return {
            "success": True,
            "faceDetected": face_ok,
            "bodyVerified": body_ok,
            "ready": face_ok and body_ok,
            "employee": camera_state.last_face_employee,
            "bodyScore": camera_state.body_score,
            "cameraRunning": camera_state.camera_running,
            "mode": camera_state.current_mode
        }


# =====================================================
# STOP CAMERA
# =====================================================
@app.post("/stop-camera")
def stop_camera():

    with camera_state.state_lock:
        camera_state.stop_camera = True

    return {
        "success": True,
        "msg": "Đã gửi lệnh tắt camera"
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)