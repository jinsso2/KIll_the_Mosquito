using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KillTheMosquito
{
    public partial class Form1 : Form
    {
        // 꼭 정해져야 하는 것? (모기 스피드, 모기 개수, 모기 출현 빈도, 파리채 스피드)

        // 파리채 이동방식? 좌표이동 or 빠른 스피드로 왕복

        // 설정들이 중간에 변경되지 않도록 상수 처리

        // 모기
        const int MOGI_NUM = 8;     // 모기 최대 마릿수

        // 모기 구조체 필요한 것? 생사여부, 위치 좌표, 속도, 방향
        // 모기(MOGI) 구조체 정의
        struct MOGI
        {
            public bool exist; // 생사여부
            public int x, y;   // 위치 좌표
            public int speed;  // 속도
            public int direction; // 모기 방향
        } MOGI[] mogi = new MOGI[MOGI_NUM]; // 모기 여러마리 출현 가능

        // 모기, 파리채 속성
        // 너비, 높이
        const int fW = 200;  // 파리채 너비
        const int fH = 278;  // 파리채 높이
        const int mW = 64;  // 모기 너비
        const int mH = 67;  // 모기 높이

        // 파리채 위치 y좌표
        int fX = 600;
        int fY = 600;

        // 점수
        double score = 0;   // 현재 점수
        static long record_score = 0;    // 최종 점수

        // 목숨
        float life = 10;

        // 게임 결과 관련 문자열
        string gameoverstr = "게임 오버";   
        string newRecord = "최고기록!!";

        // 사운드
        SoundPlayer hit, mogisnd;    // 타격음
        
        // 랜덤 값
        Random random = new Random();

        // 게임 전체 영역에 대한 Bitmap 객체
        Bitmap hFlapper, hMogi, hBackground, hGameover;
        Bitmap hArea = new Bitmap(1200, 800);    

        //키 이벤트를 처리하기 위해 필요함
        [DllImport("User32.dll")]

        //키보드로부터 입력한 키값을 얻어오는 윈도우 기반 메소드
        private static extern short GetKeyState(int nVirtKey);

        //사운드를 처리하기 위해 필요
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);
        /*
        * 미디어를 컨트롤할 인터페이스           
        * 첫 번째 매개변수 : 작동 명령
        * 두 번째 매개변수 : 결과 정보를 받을 문자열 변수 지정
        * 세 번째 매개변수 : 두 번째 전달인자에서 지정한 변수에 정보가 들어갈 최대 크기 -> 두 번째가 Null이면 세 번째도 0
        * 네 번째 매개변수 : 함수 처리가 완료 된후 해당 처리를 받을 callback 메소드 지정, 없으면 0
        */

        public Form1()
        {
            InitializeComponent();
        }
       
        private void Form1_Load(object sender, EventArgs e)     // 폼이 로드 될 때
        {
            this.Size = new Size(1200, 900);   // 폼 사이즈 조절

            hBackground = Properties.Resource.background;   // 배경 리소스 등록    
            hMogi = Properties.Resource.mogi;               // 모기 리소스 등록
            hGameover = Properties.Resource.gameover;       // 게임오버 배경 리소스 등록
            hFlapper = Properties.Resource.flapper;         // 파리채 리소스 등록

            // 효과음 리소스 등록
            hit = new SoundPlayer(Properties.Resource.hit);
            mogisnd = new SoundPlayer(Properties.Resource.mogisound);

            StartGame();
        }

        private void StartGame()
        {
            // 게임 시작 시 초기화
            for(int  i = 0; i < MOGI_NUM; i++)
            {
                mogi[i].exist = false;  // 모기 존재 여부 초기화
            }

            // 배경음악 실행
            mogisnd.Stop();
            mciSendString("open \"" + "../../../resource/background.mp3" + "\" type mpegvideo alias MediaFile", null, 0, IntPtr.Zero);
            mciSendString("play MediaFile REPEAT", null, 0, IntPtr.Zero);

            fY = 600;   // 플레이어 위치 초기화
            score = 0;  // 점수 초기화
            life = 10;  // 목숨 초기화
            timer1.Start(); // 타이머 시작
        }
     
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (hArea != null)
                {
                    e.Graphics.DrawImage(hArea, 0, 0);  // 이미지 영역 그려주기
                }
            }
            catch(Exception exception) { }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Return:   // 엔터키가 입력되었다면
                    StartGame();    // 게임시작 함수 실행
                    break;  // 조건문 종료
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 배경색으로 지우지 않고, 아무런 기능도 하지 않도록 깜빡임 현상 제거
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            MOGI localMogi; // 모기 구조체 선언

            Graphics g = Graphics.FromImage(hArea); // 그래픽 객체 얻어오기
            g.DrawImage(hBackground, 0, 0); // 배경 그리기
            g.DrawImage(hFlapper, fX - fW / 2, fY - fH / 2); // 파리채 그리기

            int i, j; // 반복문 사용을 위한 변수 선언

            // 플레이어 조작
            if (GetKeyState((int)Keys.Space) < 0) // GetKeyState : 키가 눌리면 음수 반환
            {
                fY = 300;    // 지정한 좌표로 이동
            }
            else  // 키가 떼졌을 때
            {
                fY = 600; // 원위치로
            }

            // 모기 랜덤 생성
            if(random.Next(10) == 0)
            {
                for (i = 0; i < MOGI_NUM && mogi[i].exist == true; i++){; } // i값 설정

                if (i != MOGI_NUM)      // i가 최대 모기 수와 같지 않으면
                {
                    if(random.Next(2) == 1) 
                    {
                        mogi[i].x = mW;     // 출현 위치
                        mogi[i].direction = 1;  // 방향 설정
                    }
                    else
                    {
                        mogi[i].x = ClientSize.Width - mW / 2; // 출현 위치
                        mogi[i].direction = -1; // 방향 설정
                    }
                    mogi[i].y = 200;    // 높이
                    mogi[i].speed = random.Next(60) + 2; // 랜덤 속도
                    mogi[i].exist = true;   // 존재한다
                }
            }

            if (random.Next(15) == 0)
            {
                for (i = 0; i < MOGI_NUM && mogi[i].exist == true; i++) {; } // i값 설정

                if (i != MOGI_NUM)      // i가 최대 모기 수와 같지 않으면
                {
                    if (random.Next(2) == 1)
                    {
                        mogi[i].x = mW;     // 출현 위치
                        mogi[i].direction = 1;  // 방향 설정
                    }
                    else
                    {
                        mogi[i].x = ClientSize.Width - mW / 2; // 출현 위치
                        mogi[i].direction = -1; // 방향 설정
                    }
                    mogi[i].y = 500;    // 높이
                    mogi[i].speed = random.Next(50) + 10; // 랜덤 속도
                    mogi[i].exist = true;   // 존재한다
                }
            }

            for (j = 0; j < MOGI_NUM; j++)
            {
                if(mogi[j].exist == false)
                {
                    continue;   // 모기가 없으면 건너뜀
                }
                mogi[j].x += mogi[j].speed * mogi[j].direction; // 해당 방향과 속도로 이동

                if(mogi[j].x < 0 || mogi[j].x > ClientSize.Width)   // 모기 위치가 0보다 작거나 1200보다 크면
                {
                    mogisnd.Play();
                    mogi[j].exist = false;  // 없앰
                    life--; // 목숨 하나 감소
                }
                else
                {
                    g.DrawImage(hMogi, mogi[j].x - mW / 2, mogi[j].y); // 계속 그리기
                }

                // 충돌 체크
                Rectangle mogirt, flapperrt, irt;   // 사각형 구조체 선언

                // 파리채와 모기 충돌 체크    
                
                flapperrt = new Rectangle(fX-20, fY - fW / 2, fW-150, fH); // 파리채 사각형 설정

                for (int k = 0; k < MOGI_NUM; k++)
                {
                    if (mogi[k].exist == false) continue; // 모기 없으면 건너뜀
                    // 모기가 있으면 모기 사각형 찾기

                    localMogi = mogi[k];    // 모기 개수 연산
                    int w = mW; // 모기 너비 저장
                    int h = mH; // 모기 높이 저장
                    mogirt = new Rectangle(localMogi.x - w / 2, localMogi.y, w, h); // 모기 사각형 설정

                    // 사각형 충돌판정 설정
                    irt = Rectangle.Intersect(flapperrt, mogirt);   // 사각형 교차 설정
                    if (irt.IsEmpty == false)   // 교차한다면
                    {
                        mogi[k].exist = false;  // 모기 삭제
                        hit.Play();
                        score = score + 100;    // 점수 추가
                        if (record_score < score)   // 최고점수 갱신
                        {
                            record_score = record_score + 100;  // 점수는 모기 마리 당 100씩 증가
                        }     
                    }
                }

                // UI부분
                Font font1 = new System.Drawing.Font(new FontFamily("메이플스토리"), 20, FontStyle.Bold); // 폰트 설정

                // 문자 그려주기
                g.DrawString("점수 : " + score.ToString(), font1, Brushes.DarkBlue, new PointF(10, 20));  // 점수 UI
                g.DrawString("신기록 : " + record_score.ToString(), font1, Brushes.DarkBlue, new PointF(960, 20)); // 신기록 UI
                g.DrawString("목숨 : " + life.ToString(), font1, Brushes.DarkBlue, new PointF(550, 750)); // 목숨 UI  
                
                if(score >= 100)
                {
                    if (score == record_score)
                    {
                        g.DrawString(newRecord.Substring(0, 5), font1, Brushes.DarkBlue, new PointF(830, 20)); // 신기록 UI
                    }
                }
                
                // 게임오버 
                if (life == 0)  // 목숨이 0이되면
                {
                    mciSendString("stop MediaFile", null, 0, IntPtr.Zero);  // 배경음악 정지
                    mogisnd.PlayLooping();
                    g.Clear(Color.White);   // 화면을 지우고
                    GameOver(); // 게임오버 함수 실행
                    timer1.Stop();  // 타이머 멈춤
                     
                    try
                    {
                        // 신기록 달성 시 저장
                        if (score == record_score)
                        {
                            using (StreamWriter sw = new StreamWriter("../../score.txt"))   // score.txt로 저장
                            {
                                sw.Write(record_score.ToString());  // 최고 점수를 txt로 저장
                                MessageBox.Show("신기록을 달성하여 기록 되었습니다");
                            }
                        }
                    }
                    // 예외 처리
                    catch(Exception exception)
                    {
                        string errorMsg = "예외 발생";
                        errorMsg = errorMsg + "\n" + "exception type: " + exception.GetType() + "\n" + exception.Message + "\n스택트레이스: " + exception.StackTrace;
                        using (FileStream fs = new FileStream("../../error.log", FileMode.Create))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Serialize(fs, errorMsg);                         
                            MessageBox.Show("exception이 저장되었습니다.");
                        }
                    }
                }      
            }
            Invalidate();
        }
      
        // 게임오버 화면 출력
        private void GameOver()
        {
            Graphics g = Graphics.FromImage(hArea); // 그래픽 객체 받아오기
            g.DrawImage(hGameover, 0, 0);   // 게임오버 이미지 불러오기
            
            // 저장된 최대 점수를 불러옴
            string otherScore = "";     // 다른 점수를 저장할 문자열 선언
            using (StreamReader sr = new StreamReader("../../score.txt"))   // 지정한 txt파일을 불러옴
            {
                otherScore = sr.ReadToEnd();    // 선언한 문자열에 불러온 txt파일을 읽어와 저장
            }
            Font font = new System.Drawing.Font(new FontFamily("메이플스토리"),50,FontStyle.Bold);    // 폰트 설정
            g.DrawString(gameoverstr, font, Brushes.White, new PointF(300, 150));   // 게임오버 UI
            g.DrawString("모기에게 뜯겼습니다..", font, Brushes.White, new PointF(300, 500));    // 게임오버 사유 설명 UI
            g.DrawString("점수 : " + score.ToString(), font, Brushes.DarkBlue, new PointF(300, 350)); // 획득한 점수 표시 UI
            g.DrawString("최고점수 : " + Math.Max(score, record_score) as string, font, Brushes.DarkBlue, new PointF(300, 250));   // 최고 점수 표시
            g.DrawString("다른 유저의 점수 : " + otherScore, font, Brushes.DarkBlue, new PointF(200, 650));    // 저장된 다른 최고 점수를 불러와 출력
        }
    }
}