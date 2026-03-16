import threading

# ================= CAMERA SOURCES =================
face_source = 0
body_source = 0   # có thể khác cam

# ================= SYSTEM MODE =================
current_mode = None
target_employee = None

# ================= CAMERA CONTROL =================
camera_running = False
stop_camera = False

# ================= FACE RESULT =================
last_face_employee = None
last_face_time = 0

# ================= BODY RESULT =================
body_verified = False
body_score = 0.0

# ================= DETECT STATE =================
face_detected = False
body_detected = False

# ================= THREAD LOCK =================
state_lock = threading.Lock()