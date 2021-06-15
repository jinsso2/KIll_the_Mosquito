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
namespace KillTheMosquito
{
    public partial class Form1 : Form
    {
        // 꼭 정해져야 하는 것? (모기 스피드, 모기 개수, 모기 출현 빈도, 파리채 스피드)
        // 파리채 이동방식? 좌표이동 or 빠른 스피드로 왕복

        // 설정들이 중간에 변경되지 않도록 상수 처리
        
        // 모기
        // 모기 속성
        const int MOGI_NUM = 10;    // 모기 최대 개수

        // 모기 구조체 필요한 것? 생사여부, 위치 좌표, 속도, 방향

        // 파리채(플레이어)
        // 파리채 속성
        const int FLAPPER_SPEED = 30;   // 파리채 이동 속도
        const int FLAPPER_GAP = 100;    // 파리채를 다시 휘두를 간격


        // 파리채 구조체 필요한 것? 속도, 위치 좌표
        // 구조체 정의
        // 모기(MOGI) 구조체 정의
        struct MOGI
        {
            public bool exist;      // 생사 여부
            public int x, y;        // 위치 좌표
            public int speed;       // 속도
            public int direction;   // 방향
        }
        MOGI[] mogi = new MOGI[MOGI_NUM];   // 모기 여러마리 출현 가능(10마리)


        // 모기, 파리채 속성
        // 너비, 높이
        const int fW = 80;  // 플레이어 너비
        const int fH = 240; // 플레이어 높이
        const int mW = 60;  // 모기 너비
        const int mH = 45;  // 모기 높이

        // x, y 좌표
        int fX = 600;       // 파리채 위치 x좌표
        int fY = 700;       // 파리채 위치 y좌표

        // 점수
        int score = 0;  // 현재 점수
        static int record_score = 0;    // 신기록, 시작 시 0이어야하기 때문에 static

        // 사운드
        SoundPlayer sndBomb;    // 파리채 휘두르는 소리, 타격 소리

        // 랜덤 값
        Random random = new Random();

        // 게임 전체 영역에 대한 Bitmap 객체
        Bitmap hMogi, hfalpper, hBackGround, hgameover;
        Bitmap hArea = new Bitmap(800, 450);


        //키 이벤트를 처리하기 위해 필요함
        [DllImport("User32.dll")]

        //키보드로부터 입력한 키값을 얻어오는 윈도우 기반 메소드
        private static extern short GetKeyState(int nVirtKey);

        //사운드를 처리하기 위해 필요
        [DllImport("winmm.dll")]

        //사운드 음원 재생 및 정지와 같은 기능을 수행하기 위한 윈도우 기반 메소드
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
            this.Size = new Size(800, 450); // 폼 사이즈 조정

            // 리소스 등록 (폼이 로드 될 때 한번만)
            hBackGround = Properties.Resource.background;   // 배경화면 리소스 등록
            hgameover = Properties.Resource.gameover;       // 게임오버 화면 리소스 등록
            hMogi = Properties.Resource.mogi;               // 모기 리소스 등록
            hfalpper = Properties.Resource.flapper;         // 파리채 리소스 등록
            
            // 파리채 휘두르는 효과음 등록

            StartGame();
        }

        private void StartGame()
        {
            // 게임이 시작될 시 초기화
            for (int i = 0; i < MOGI_NUM; i++)
            {
                mogi[i].exist = false;
                // 모기의 존재여부 false로 초기화
            }

            // 배경음악 재생

            // 스코어 초기화
            score = 0;

            // 타이머 시작
            timer1.Start();
        }

        private void StopGame()
        {
            Graphics g = Graphics.FromImage(hArea); // 그래픽 객체 받아오기
            g.DrawImage(hgameover, 0, 0); // 그려주기
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if(hArea != null)
            {
                e.Graphics.DrawImage(hArea, 0, 0);  // 전체적인 너비의 이미지 영역 그리기
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 배경색으로 지우지 않고, 아무런 기능도 하지 않도록 깜빡임 현상 제거
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Graphics g = Graphics.FromImage(hArea); // 그래픽 객체 받아오기 
            g.DrawImage(hBackGround, 0, 0); // 배경 그려주기
            g.DrawImage(hfalpper, fX - fW / 2, fY - fH / 2);    // 파리채 그려주기
        }
    }
}
