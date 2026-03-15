
import cv2
import numpy as np
import datetime
import time
import random
from chamcong import camera_state
from insightface.app import FaceAnalysis
from PIL import ImageFont, ImageDraw, Image
from chamcong.ketnoi_SQL import get_ketnoi
from discord_notify import send_unknown_alert


# ======================= CONFIG =======================
# ======================= CẤU HÌNH CAMERA =======================

FRAME_SIZE = (640, 480)
# Kích thước frame sau khi resize (giảm tải xử lý AI, tăng FPS)


# ======================= NGƯỠNG NHẬN DIỆN =======================

SIM_THRESHOLD = 0.76
# Ngưỡng similarity cơ bản để xem 2 khuôn mặt là giống nhau
# (cosine similarity)
# > 0.76 → có khả năng cùng người

FAST_THRESHOLD = 0.85
# Ngưỡng nhận diện nhanh (độ tin cậy cao)
# Nếu vượt mức này có thể xác nhận gần như chắc chắn


# ======================= TĂNG ĐỘ CHÍNH XÁC =======================

FACE_BUFFER_SIZE = 2
# Số frame gần nhất dùng để lấy trung bình điểm nhận diện
# Giúp tránh nhận sai do 1 frame bị nhiễu

FACE_STRICT_THRESHOLD = 0.80   # ~90%
# Ngưỡng xác nhận cuối cùng để CHẤM CÔNG
# Trung bình điểm nhận diện phải >= giá trị này
# → đảm bảo độ chính xác cao (~90%)

FACE_NORMAL_THRESHOLD = 0.76
# Ngưỡng dự phòng (fallback)
# Dùng khi cần nhận diện ở mức bình thường


# ======================= HIỆU NĂNG XỬ LÝ =======================

DETECT_INTERVAL = 0.03
# Khoảng thời gian (giây) giữa 2 lần detect face
# 0.25s ≈ 4 lần detect / giây
# Giảm lag CPU/GPU

LOCK_TIME = 2.5
# Sau khi chấm công thành công:
# khóa kết quả hiển thị trong 2.5 giây
# tránh chấm công lặp nhiều lần


# ======================= HIỂN THỊ TEXT =======================

TEXT_COOLDOWN = 1.8
# Thời gian chờ trước khi cho phép đổi text hiển thị

TEXT_RANDOM_INTERVAL = 3.0
# Sau mỗi 3 giây sẽ random câu thông báo mới
# (welcome text, fortune text...)
FONT_28 = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 28)
FONT_32 = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 32)

# ======================= TEXT =======================
WELCOME_TEXTS = [
    "Hello boss ",
    "Lại là bạn à ",
    "Chào người quen ",
    "Hôm nay nhìn xịn ghê ",
    "Đi làm hả "
]

LATE_TEXTS = [
    "Đi trễ nha ",
    "Báo thức có kêu không ",
    "Hơi trễ xíu rồi ",
    "Lại trễ nữa rồi "
]

ONTIME_TEXTS = [
    "Đúng giờ luôn, xịn ",
    "Nhân viên gương mẫu ",
    "Hôm nay siêng ghê ",
    "Đi làm sớm thế "
]

FAIL_TEXTS = [
    "Ai đây ta ",
    "Mặt quen mà tên lạ ",
    "Camera chưa nhớ bạn ",
    "Đứng gần lại coi ",
    "Nhìn rõ hơn xíu "
]

MULTI_TEXTS = [
    "Ủa đông vậy ",
    "Camera chỉ nhìn 1 người thôi ",
    "Tách ra coi ",
    "Đi theo cặp hả "
]

POSE_TEXTS = [
    "Quay nhẹ mặt nữa coi ",
    "Gần đúng rồi đó ",
    "Xoay thêm xíu nữa "
]
# ======================= TEXT CACHE =======================
_TEXT_CACHE = {}

def get_text_3s(text_list, key):
    now = time.time()
    if key not in _TEXT_CACHE:
        _TEXT_CACHE[key] = {"text": random.choice(text_list), "time": now}
        return _TEXT_CACHE[key]["text"]

    if now - _TEXT_CACHE[key]["time"] >= TEXT_RANDOM_INTERVAL:
        _TEXT_CACHE[key]["text"] = random.choice(text_list)
        _TEXT_CACHE[key]["time"] = now

    return _TEXT_CACHE[key]["text"]

# ======================= 100 CÂU COI BÓI =======================
FORTUNES = [
    # 🌞 SUN – tích cực
    ("Hôm nay làm việc trôi chảy bất ngờ", "sun.png"),
    ("Có người âm thầm hỗ trợ bạn đó", "sun.png"),
    ("Tinh thần tốt thì việc cũng tốt", "sun.png"),
    ("Cười nhiều sẽ gặp may", "sun.png"),
    ("Hôm nay hợp teamwork", "sun.png"),
    ("Việc khó rồi cũng xuôi", "sun.png"),
    ("Một khởi đầu dễ chịu", "sun.png"),
    ("Năng lượng hôm nay khá ổn", "sun.png"),
    ("Cứ làm từ từ là tới", "sun.png"),
    ("Hôm nay dễ được khen", "sun.png"),

    # 💰 MONEY – tiền bạc
    ("Tiền chưa tới nhưng đang trên đường", "money.png"),
    ("Đừng bao người khác hôm nay", "money.png"),
    ("Chi tiêu khôn ngoan là thắng lớn", "money.png"),
    ("Sắp có tin vui về tiền nong", "money.png"),
    ("Ví mỏng nhưng tâm hồn giàu", "money.png"),
    ("Hôm nay không nên mua linh tinh", "money.png"),
    ("Giữ tiền kỹ là phước", "money.png"),
    ("Có khoản chi bất ngờ", "money.png"),
    ("Tiền vào chậm nhưng chắc", "money.png"),
    ("Đếm tiền chưa đã, đếm kinh nghiệm trước", "money.png"),

    # ❤️ HEART – cảm xúc
    ("Có người đang nhớ tới bạn", "heart.png"),
    ("Hôm nay dễ được quan tâm bất ngờ", "heart.png"),
    ("Nói chuyện nhẹ nhàng là ghi điểm", "heart.png"),
    ("Đừng suy nghĩ nhiều quá nha", "heart.png"),
    ("Một nụ cười bằng mười lời nói", "heart.png"),
    ("Giữ cảm xúc ổn là đủ", "heart.png"),
    ("Hôm nay dễ nhạy cảm hơn thường ngày", "heart.png"),
    ("Có người hiểu bạn hơn bạn nghĩ", "heart.png"),
    ("Đừng tự áp lực mình", "heart.png"),
    ("Tâm trạng quyết định hiệu suất", "heart.png"),

    # ⚠ WARN – nhắc nhở
    ("Nhớ coi giờ kẻo quên việc", "warn.png"),
    ("Bình tĩnh là chìa khóa", "warn.png"),
    ("Đừng nóng, việc gì cũng có cách", "warn.png"),
    ("Hôm nay nên quan sát nhiều hơn", "warn.png"),
    ("Uống nước đi, não đang khô đó", "warn.png"),
    ("Làm chậm lại một chút", "warn.png"),
    ("Đừng hứa khi chưa chắc", "warn.png"),
    ("Có thể bị phân tâm", "warn.png"),
    ("Cẩn thận mấy chi tiết nhỏ", "warn.png"),
    ("Đừng để cảm xúc lái tay", "warn.png"),

    # 😂 FUN – vui nhộn
    ("Hôm nay bạn nghiêm túc hơi dư", "fun.png"),
    ("Đi làm mà tâm trí đi chơi", "fun.png"),
    ("Cố tỏ ra nguy hiểm", "fun.png"),
    ("Hợp ngồi im giả bộ bận", "fun.png"),
    ("Cười hoài coi chừng bị tưởng rảnh", "fun.png"),
    ("Nhìn có vẻ bận chứ trong đầu rỗng", "fun.png"),
    ("Làm thì ít, nghĩ thì nhiều", "fun.png"),
    ("Đang online nhưng tinh thần offline", "fun.png"),
    ("Hôm nay hợp vai phụ", "fun.png"),
    ("Đừng diễn sâu quá", "fun.png"),

    # ☕ COFFEE – năng lượng thấp
    ("Có vẻ bạn cần thêm cà phê", "coffee.png"),
    ("Não chưa load xong", "coffee.png"),
    ("Uống nước hay cà phê đi", "coffee.png"),
    ("Năng lượng đang ở mức tiết kiệm", "coffee.png"),
    ("Chạy bằng ý chí là chính", "coffee.png"),
    ("Đầu óc hơi lag", "coffee.png"),
    ("Cà phê không phải xa xỉ lúc này", "coffee.png"),
    ("Hôm nay hợp làm việc nhẹ", "coffee.png"),
    ("Đừng ép bản thân quá", "coffee.png"),
    ("Ngáp nhiều là dấu hiệu", "coffee.png"),

    # 🕒 TIME – thời gian
    ("Thời gian trôi nhanh hơn bạn nghĩ", "time.png"),
    ("Nhớ kiểm tra deadline", "time.png"),
    ("Làm sớm đỡ mệt", "time.png"),
    ("Đừng để nước tới chân", "time.png"),
    ("Có việc cần làm ngay", "time.png"),
    ("Chậm một chút cũng không sao", "time.png"),
    ("Canh giờ kỹ là ổn", "time.png"),
    ("Hôm nay dễ quên giờ", "time.png"),
    ("Đúng lúc là chìa khóa", "time.png"),
    ("Tập trung vào hiện tại", "time.png"),
]

# ======================= MODEL =======================
face_app = FaceAnalysis(name="buffalo_s")
face_app.prepare(ctx_id=0, det_size=(640, 640))

# ======================= DB =======================
conn = get_ketnoi()

# ======================= FACE DB CACHE =======================
FACE_DB = []

def reload_face_db():
    FACE_DB.clear()
    cur = conn.cursor()
    cur.execute("""
        SELECT f.EmployeeId, e.FullName, f.Embedding
        FROM FaceEmbeddings f
        JOIN Employees e ON f.EmployeeId = e.Id
    """)
    for emp_id, name, emb in cur.fetchall():
        FACE_DB.append((emp_id, name, np.frombuffer(emb, np.float32)))

# ======================= UTILS =======================
def cosine_similarity(a, b):
    return float(np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b)))

def put_text_vi(frame, text, pos, font, color):
    img = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    draw = ImageDraw.Draw(img)
    draw.text(pos, text, font=font, fill=color)
    return cv2.cvtColor(np.array(img), cv2.COLOR_RGB2BGR)

def draw_icon(frame, icon_path, x, y, size=36):
    icon = Image.open(icon_path).convert("RGBA").resize((size, size))
    img = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    img.paste(icon, (x, y), icon)
    return cv2.cvtColor(np.array(img), cv2.COLOR_RGB2BGR)

def draw_text_with_icon(
    frame,
    text,
    icon,
    pos=(20, 110),
    font=FONT_28,
    color=(255, 200, 0),
    icon_size=32,
    gap=8
):
    x, y = pos

    # đo chiều cao chữ
    dummy_img = Image.new("RGB", (10, 10))
    dummy_draw = ImageDraw.Draw(dummy_img)
    bbox = dummy_draw.textbbox((0, 0), text, font=font)
    text_h = bbox[3] - bbox[1]

    # căn icon giữa theo chữ
    icon_y = y + (text_h - icon_size) // 2

    # vẽ icon
    frame = draw_icon(
        frame,
        f"icons/{icon}",
        x,
        icon_y,
        size=icon_size
    )

    # vẽ text bên phải icon
    frame = put_text_vi(
        frame,
        text,
        (x + icon_size + gap, y),
        font,
        color
    )

    return frame
# ======================= DUTY DB =======================
def has_duty_schedule(employee_id, duty_shift_id, duty_date):
    cur = conn.cursor()
    cur.execute("""
        SELECT 1
        FROM DutySchedules
        WHERE EmployeeId = ?
          AND DutyShiftId = ?
          AND CAST(DutyDate AS DATE) = ?
    """, employee_id, duty_shift_id, duty_date)

    return cur.fetchone() is not None


def save_duty_attendance(employee_id, duty_shift_id, duty_date, now):
    cur = conn.cursor()
    cur.execute("""
        IF NOT EXISTS (
            SELECT 1 FROM DutyAttendance
            WHERE EmployeeId = ?
              AND DutyShiftId = ?
              AND CAST(DutyDate AS DATE) = ?
        )
        INSERT INTO DutyAttendance
        (EmployeeId, DutyShiftId, CheckTime, DutyDate, Status)
        VALUES (?, ?, ?, ?, N'Đúng giờ')
    """,
    employee_id, duty_shift_id, duty_date,
    employee_id, duty_shift_id, now, duty_date)

    conn.commit()

# ======================= SHIFT =======================
def get_work_shift(now):
    t = now.time()
    if datetime.time(7, 30) <= t <= datetime.time(11, 30):
        return 1, "Ca sáng"
    if datetime.time(13, 30) <= t <= datetime.time(20, 0):
        return 2, "Ca chiều"
    return None, None

# ======================= DUTY SHIFT =======================
def get_duty_shift(now: datetime.datetime):
    """
    Trả về: (duty_shift_id, duty_name, duty_date)
    """
    t = now.time()

    # Ca trực ngày: 07:00 → 17:00
    if datetime.time(7, 0) <= t < datetime.time(17, 0):
        return 1, "Ca trực ngày", now.date()

    # Ca trực đêm: 17:00 → 07:00 hôm sau
    if t >= datetime.time(17, 0):
        return 2, "Ca trực đêm", now.date()

    if t < datetime.time(7, 0):
        return 2, "Ca trực đêm", now.date() - datetime.timedelta(days=1)

    return None, None, None

def get_status(shift_id, now):
    if shift_id == 1:
        return "Trễ" if now.time() > datetime.time(7, 35) else "Đúng giờ"
    if shift_id == 2:
        return "Trễ" if now.time() > datetime.time(13, 35) else "Đúng giờ"


def put_text_wrap(frame, text, pos, font, color, max_width, line_spacing=6):
    img = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    draw = ImageDraw.Draw(img)

    x, y = pos
    start_y = y
    words = text.split(" ")
    line = ""

    for word in words:
        test_line = line + word + " "
        bbox = draw.textbbox((0, 0), test_line, font=font)
        w = bbox[2] - bbox[0]
        h = bbox[3] - bbox[1]

        if w <= max_width:
            line = test_line
        else:
            draw.text((x, y), line, font=font, fill=color)
            y += h + line_spacing
            line = word + " "

    if line:
        draw.text((x, y), line, font=font, fill=color)
        y += h

    return cv2.cvtColor(np.array(img), cv2.COLOR_RGB2BGR), y


# ======================= FORTUNE CACHE =======================
FORTUNE_CACHE = {}
GLOBAL_FORTUNE_CACHE = {}

def get_fortune(emp_id, shift_id, date):
    key = (emp_id, shift_id, date)
    if key not in FORTUNE_CACHE:
        FORTUNE_CACHE[key] = random.choice(FORTUNES)
    return FORTUNE_CACHE[key]

def get_global_fortune(shift_id, date):
    key = (shift_id, date)
    if key not in GLOBAL_FORTUNE_CACHE:
        GLOBAL_FORTUNE_CACHE[key] = random.choice(FORTUNES)
    return GLOBAL_FORTUNE_CACHE[key]

# ======================= STATE =======================
last_detect_time = 0
locked = False
lock_start = 0
faces = []
best_match = None
best_score = 0
last_result = None
# ===== FACE SCORE BUFFER (90% ACCURACY) =====
face_score_buffer = []

def process_checkin(employee_id, now):
    messages = []
    work_status = None
    inserted = False

    # ===== CA LÀM =====
    shift_id, shift_name = get_work_shift(now)
    if shift_id:
        work_status = get_status(shift_id, now)

        cur = conn.cursor()
        cur.execute("""
            SELECT 1 FROM Attendance
            WHERE EmployeeId = ?
              AND ShiftId = ?
              AND CAST(CheckTime AS DATE) = CAST(? AS DATE)
        """, employee_id, shift_id, now)

        if cur.fetchone():
            messages.append(shift_name)
            return messages, work_status, True, "Đã chấm công ca làm rồi"

        cur.execute("""
            INSERT INTO Attendance
            (EmployeeId, ShiftId, CheckTime, Status, SimilarityScore)
            VALUES (?, ?, ?, ?, 0.0)
        """,
        employee_id, shift_id, now, work_status)

        conn.commit()
        messages.append(shift_name)
        inserted = True

    # ===== CA TRỰC =====
    duty_id, duty_name, duty_date = get_duty_shift(now)
    if duty_id and has_duty_schedule(employee_id, duty_id, duty_date):
        save_duty_attendance(employee_id, duty_id, duty_date, now)
        messages.append(duty_name)
        inserted = True

    if not inserted:
        return [], work_status, False, "Chưa chấm công được"

    return messages, work_status, True, None



def resolve_shift(now, employee_id=None):
    """
    Ưu tiên ca trực nếu nhân viên CÓ lịch trực
    """
    # ===== CA TRỰC =====
    duty_id, duty_name, duty_date = get_duty_shift(now)

    if duty_id and employee_id:
        if has_duty_schedule(employee_id, duty_id, duty_date):
            return {
                "key": f"duty_{duty_id}",
                "label": duty_name,
                "date": duty_date,
                "type": "duty"
            }

    # ===== CA LÀM =====
    work_id, work_name = get_work_shift(now)
    if work_id:
        return {
            "key": f"work_{work_id}",
            "label": work_name,
            "date": now.date(),
            "type": "work"
        }

    return None




# ======================= MAIN =======================
def checkin_frame(frame):
    global last_detect_time, locked, lock_start
    global faces, best_match, best_score, last_result

    if not FACE_DB:
        reload_face_db()

    now = datetime.datetime.now()
    frame = cv2.resize(frame, FRAME_SIZE)

    # ===== TIME =====
    frame = put_text_vi(
        frame,
        now.strftime("%d/%m/%Y | %H:%M:%S"),
        (20, 10),
        FONT_28,
        (0, 255, 255)
    )

    # ===== RESOLVE SHIFT (TRUNG TÂM) =====
    emp_id = best_match[0] if best_match else None
    shift = resolve_shift(now, emp_id)

    shift_id   = shift["key"] if shift else None
    shift_name = shift["label"] if shift else None
    shift_date = shift["date"] if shift else None

    # ===== LOCK – GIỮ FRAME SAU CHECKIN =====
    if locked and time.time() - lock_start < LOCK_TIME:
        r = last_result
        frame = put_text_vi(frame, r["name"], (20, 40), FONT_32, (0, 255, 0))
        frame = put_text_vi(
            frame,
            f'{r["shift"]} - {r["status"]}',
            (20, 80),
            FONT_32,
            (0, 255, 0)
        )
        frame = put_text_vi(frame, r["fortune"], (20, 130), FONT_28, (255, 200, 0))
        frame = draw_icon(frame, f'icons/{r["icon"]}', 20, 170)
        return frame
    else:
        locked = False

    # ===== DETECT FACE =====
    # ===== DETECT FACE =====
    if time.time() - last_detect_time > DETECT_INTERVAL:
        faces = face_app.get(frame)
        last_detect_time = time.time()
        best_match = None
        best_score = 0

        if faces:
            face = max(
                faces,
                key=lambda f: (f.bbox[2] - f.bbox[0]) * (f.bbox[3] - f.bbox[1])
            )

            emb = face.normed_embedding

            for emp_id, name, db_emb in FACE_DB:
                sim = cosine_similarity(emb, db_emb)
                if sim > best_score:
                    best_score = sim
                    best_match = (emp_id, name)

            # ===== BUFFER SCORE (QUAN TRỌNG) =====
            face_score_buffer.append(best_score)
            if len(face_score_buffer) > FACE_BUFFER_SIZE:
                face_score_buffer.pop(0)
        else:
            face_score_buffer.clear()

    # ===== AVG FACE SCORE (ỔN ĐỊNH) =====
    avg_face_score = (
        sum(face_score_buffer) / len(face_score_buffer)
        if face_score_buffer else 0
    )

    # ===== MULTI / IDLE =====
    if len(faces) != 1:
        if shift_id:
            ft, ic = get_fortune(0, shift_id, shift_date)  # emp_id = 0

            frame = put_text_vi(
                frame,
                get_text_3s(MULTI_TEXTS, "MULTI"),
                (20, 60),
                FONT_32,
                (255, 180, 0)
            )
            frame = draw_text_with_icon(
                frame,
                text=ft,
                icon=ic,
                pos=(20, 110),
                font=FONT_28
            )
        return frame

    # ===== UNKNOWN / FAIL =====
    # ===== UNKNOWN / FAIL =====
    # ===== UNKNOWN / FAIL (CHỈ KHI KHÔNG BIẾT AI) =====
    if best_match is None:

        send_unknown_alert()

        # ===== LUÔN XÁC ĐỊNH CA =====
        if shift is None:
            # nếu chưa có ca → lấy ca làm hiện tại theo giờ
            work_id, work_name = get_work_shift(now)
            if work_id:
                shift_id = f"work_{work_id}"
                shift_name = work_name
                shift_date = now.date()
            else:
                # nếu ngoài giờ hoàn toàn
                shift_id = "NO_SHIFT"
                shift_name = "Ngoài giờ"
                shift_date = now.date()

        ft, ic = get_fortune(0, shift_id, shift_date)

        frame, y_end = put_text_wrap(
            frame,
            f"Camera chưa nhớ bạn • {shift_name}",
            (20, 60),
            FONT_32,
            (255, 80, 80),
            max_width=600
        )

        frame = draw_text_with_icon(
            frame,
            text=ft,
            icon=ic,
            pos=(20, y_end + 10),
            font=FONT_28
        )

        return frame

    # ===== CHƯA TỚI CA (NHƯNG NHẬN DIỆN ĐƯỢC) =====
    # ===== CHƯA TỚI CA (NHƯNG NHẬN DIỆN ĐƯỢC) =====
    # ===== NHẬN DIỆN ĐƯỢC NHƯNG CHƯA TỚI CA → XEM BÓI =====
    if best_match is not None and shift_id is None:
        ft, ic = get_fortune(
            best_match[0],
            "NO_SHIFT",
            now.date()
        )

        frame = put_text_vi(
            frame,
            "Chưa chấm công • Xem bói vui thôi",
            (20, 60),
            FONT_32,
            (0, 180, 255)
        )
        frame = draw_text_with_icon(
            frame,
            text=ft,
            icon=ic,
            pos=(20, 110),
            font=FONT_28
        )
        return frame

    if avg_face_score < FACE_STRICT_THRESHOLD:
        frame = put_text_vi(
            frame,
            "Đứng yên 1 xíu nha…",
            (20, 60),
            FONT_32,
            (255, 200, 0)
        )
        return frame



    # ===== CHECKIN SUCCESS =====
    fortune_text, fortune_icon = get_fortune(
        best_match[0],
        shift_id,
        shift_date
    )

    checked, work_status, ok, reason = process_checkin(best_match[0], now)

    # vẫn cho hiển thị nếu đã chấm công
    if not ok and reason != "Đã chấm công ca làm rồi":
        frame = put_text_vi(
            frame,
            reason,
            (20, 60),
            FONT_32,
            (255, 120, 0)
        )
        return frame

    last_result = {
        "name": best_match[1],
        "shift": " + ".join(checked),
        "status": work_status or "Đúng giờ",
        "fortune": fortune_text,
        "icon": fortune_icon
    }

    with camera_state.state_lock:
        camera_state.last_face_employee = best_match[0]
        camera_state.last_face_time = time.time()

    locked = True
    lock_start = time.time()
    return frame
