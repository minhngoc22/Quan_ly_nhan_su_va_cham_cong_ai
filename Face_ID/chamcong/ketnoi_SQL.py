import pyodbc

def get_ketnoi():
    return pyodbc.connect(
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=localhost;"
        "DATABASE=FaceID_HRMS;"
        "UID=ss;"
        "PWD=123;"
        "Trusted_Connection=yes;"
    )


if __name__ == "__main__":
    try:
        conn = get_ketnoi()
        print("Kết Nối SQL thành công")
        conn.close()
    except Exception as e:
        print("❌ SQL connection failed")
        print(e)