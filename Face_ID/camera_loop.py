import cv2
from chamcong import camera_state

from chamcong.cham_cong import checkin_frame
from chamcong.Data_Face import enroll_face_step, save_face_embedding
from chamcong.thudang import enroll_body_step, save_body_embedding
from chamcong.body_check import verify_body


def camera_loop():

    face_cam = cv2.VideoCapture(camera_state.face_source)
    body_cam = cv2.VideoCapture(camera_state.body_source)

    if not face_cam.isOpened():
        print("❌ Không mở được camera")
        return

    print("📷 Camera System ON")

    while True:

        with camera_state.state_lock:
            if camera_state.stop_camera:
                break

            mode = camera_state.current_mode
            emp = camera_state.target_employee

        # ================= READ FRAME =================
        ret1, face_frame = face_cam.read()
        if not ret1:
            break

        face_frame = cv2.flip(face_frame, 1)

        if camera_state.body_source != camera_state.face_source:
            ret2, body_frame = body_cam.read()
            body_frame = cv2.flip(body_frame, 1) if ret2 else None
        else:
            body_frame = face_frame

        # ================= MODE =================

        # 👤 FACE CHECKIN
        if mode == "checkin":
            face_frame = checkin_frame(face_frame)

        # 🧍 BODY CHECK ONLY
        elif mode == "check_body":
            if body_frame is not None:
                body_frame = verify_body(body_frame)

                # ⭐ AUTO STOP khi đã verify
                with camera_state.state_lock:
                    if camera_state.body_verified:
                        print("✅ Body verified")
                        camera_state.body_verified = False

        # 👤 REGISTER FACE
        elif mode == "register_face" and emp:
            face_frame, done = enroll_face_step(face_frame, emp)

            if done:
                save_face_embedding(emp)
                print("✅ Face registration completed")
                break

        # 🧍 REGISTER BODY
        elif mode == "register_body" and emp:
            if body_frame is not None:
                body_frame, done = enroll_body_step(body_frame, emp)

                if done:
                    save_body_embedding(emp)
                    print("✅ Body registration completed")
                    break

        # ================= DISPLAY =================
        if mode == "register_face":
            cv2.imshow("Register Face", face_frame)

        elif mode == "register_body" and body_frame is not None:
            cv2.imshow("Register Body", body_frame)

        elif mode == "check_body" and body_frame is not None:
            cv2.imshow("Body Check", body_frame)

        elif mode == "checkin":
            cv2.imshow("Face Cam", face_frame)

        if cv2.waitKey(1) & 0xFF == 27:
            break

    # ================= CLEANUP =================
    face_cam.release()
    body_cam.release()
    cv2.destroyAllWindows()

    with camera_state.state_lock:
        camera_state.camera_running = False
        camera_state.stop_camera = False
        camera_state.current_mode = None
        camera_state.target_employee = None

    print("🛑 Camera System OFF")