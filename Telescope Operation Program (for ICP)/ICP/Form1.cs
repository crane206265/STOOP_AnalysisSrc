using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using ASCOM.DriverAccess;
using System.Threading;
using System.Globalization;
using System.IO;
namespace ICP
{
    public partial class Form1 : Form
    {
        // Create Telescope class 
        Telescope telescope;
        Util util;
        Task taskMonitoring;
        bool monitoring = true;
        public Form1()
        {
            InitializeComponent();
            // Form 닫기 이벤트 핸들러 추가
            this.FormClosing += Form1_FormClosing;

            // TextBox 멀티라인 속성 설정
            textBox1.Multiline = true;
            textBox1.ScrollBars = ScrollBars.Vertical;

            // 텍스트박스에 고정 너비 폰트 적용
            textBox1.Font = new Font("Consolas", textBox1.Font.Size);
        }

        // Form 닫기 이벤트 처리
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 모니터링 중지
            StopMonitoring();

            // 망원경 연결 해제
            DisconnectTelescope();
        }

        // 망원경 연결 해제 메서드
        private void DisconnectTelescope()
        {
            try
            {
                if (telescope != null && telescope.Connected)
                {
                    telescope.Connected = false;
                }
            }
            catch (Exception ex)
            {
                // 예외 처리
                MessageBox.Show("망원경 연결 해제 중 오류 발생: " + ex.Message);
            }
        }

        // 모니터링 중지 메서드
        private void StopMonitoring()
        {
            monitoring = false;

            // 작업이 완료될 때까지 잠시 대기
            if (taskMonitoring != null && taskMonitoring.Status == TaskStatus.Running)
            {
                try
                {
                    // 최대 3초 동안 작업 완료 대기
                    Task.WaitAll(new[] { taskMonitoring }, 3000);
                }
                catch (Exception)
                {
                    // 시간 초과 등의 예외 무시
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //가대를 선택하는 창을 띄운 후, 선택된 가대의 ID를 받아온다.
            string id = Telescope.Choose("");
            //가대를 선택하지 않은 경우, 중단한다.
            if (string.IsNullOrEmpty(id))
                return;
            //선택된 가대의 객체를 생성한다.
            telescope = new Telescope(id);
            textBox2.Text = id;
            //가대에 연결한다.
            telescope.Connected = true;
            //ASCOM 드라이버에서 받는 각종 가대의 정보를 보여준다.
            ShowInfo();
        }
        private void ShowInfo()
        {
            monitoring = true;
            taskMonitoring = new Task(Monitoring);
            taskMonitoring.Start();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }
        private void Monitoring()
        {
            while (monitoring)//정해진 종료시점 없이 모니터링을 해야하므로, 무한 루프를 돌리고, 외부에서 버튼을 눌러 종료
            {
                try
                {
                    if (telescope == null || !telescope.Connected)
                    {
                        // 망원경이 연결되지 않은 경우 루프 종료
                        monitoring = false;
                        break;
                    }

                    string parking = string.Empty;
                    if (telescope.AtPark == true)
                    {
                        parking = "Park";
                    }
                    else
                    {
                        parking = "Unpark";
                    }
                    string tracking = string.Empty;
                    if (telescope.Tracking == true)
                    {
                        tracking = "On";
                    }
                    else
                    {
                        tracking = "Off";
                    }
                    string slewing = string.Empty;
                    if (telescope.Slewing)
                    {
                        slewing = "Slewing";
                    }
                    else
                    {
                        slewing = "stop";
                    }
                    // 줄바꿈이 제대로 적용되도록 Environment.NewLine 사용
                    string infoofTelesope = string.Format("{0,-18} : {1}{2}", "Device Name", telescope.Name, Environment.NewLine);
                    infoofTelesope += string.Format("{0,-18} : {1}{2}", "Park/Unpark", parking, Environment.NewLine);
                    infoofTelesope += string.Format("{0,-18} : {1}{2}", "RightAscension", telescope.RightAscension.ToString(), Environment.NewLine);
                    infoofTelesope += string.Format("{0,-18} : {1}{2}", "Declination", telescope.Declination.ToString(), Environment.NewLine);
                    infoofTelesope += string.Format("{0,-18} : {1}{2}", "Altitude", telescope.Altitude.ToString(), Environment.NewLine);
                    infoofTelesope += string.Format("{0,-18} : {1}{2}", "Azimuth", telescope.Azimuth.ToString(), Environment.NewLine);
                    infoofTelesope += string.Format("{0,-18} : {1}{2}", "Slew", slewing, Environment.NewLine);
                    infoofTelesope += string.Format("{0,-18} : {1}", "Tracking", tracking);
                    //UI에 반영하려면, 아래와 같이 직접 Dispathcer를 Invoke해 주어야 한다.
                    if (!IsDisposed && textBox1 != null)
                    {
                        if (textBox1.InvokeRequired)
                        {
                            textBox1.BeginInvoke((MethodInvoker)delegate
                            {
                                if (!textBox1.IsDisposed)
                                    textBox1.Text = infoofTelesope;
                            });
                        }
                        else
                        {
                            if (!textBox1.IsDisposed)
                                textBox1.Text = infoofTelesope;
                        }
                    }

                    // 모니터링 간격 추가 (100ms)
                    Thread.Sleep(100);
                }
                catch (ObjectDisposedException)
                {
                    // 객체가 이미 해제된 경우 모니터링 중지
                    monitoring = false;
                }
                catch (Exception ex)
                {
                    // 예외 처리
                    try
                    {
                        if (!IsDisposed)
                        {
                            MessageBox.Show("모니터링 중 오류 발생: " + ex.Message);
                        }
                    }
                    catch
                    {
                        // 무시
                    }
                    monitoring = false;
                }
            }
        }
        private string selectedFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Telescope_RA_Dec.csv");

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (telescope == null)
                {
                    MessageBox.Show("가대가 연결되지 않았습니다. 먼저 연결해 주세요.", "가대 미연결", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!telescope.Connected)
                {
                    MessageBox.Show("가대가 아직 연결되지 않았습니다. 연결 후 다시 시도해 주세요.", "연결 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string ra = telescope.RightAscension.ToString(CultureInfo.InvariantCulture);
                string dec = telescope.Declination.ToString(CultureInfo.InvariantCulture);

                using (StreamWriter writer = new StreamWriter(selectedFilePath)) // 덮어쓰기
                {
                    writer.WriteLine("RA,Dec");
                    writer.WriteLine($"{ra},{dec}");
                }

                MessageBox.Show($"RA/Dec 좌표가 성공적으로 저장되었습니다.\n\n파일 경로:\n{selectedFilePath}", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RA/Dec 저장 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "RA/Dec 저장 위치 선택";
                saveFileDialog.Filter = "CSV 파일 (*.csv)|*.csv";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFileDialog.FileName = "Telescope_RA_Dec.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = saveFileDialog.FileName;
                    MessageBox.Show($"파일이 저장될 경로가 설정되었습니다:\n{selectedFilePath}", "경로 지정 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private double loadedRA = 0;
        private double loadedDec = 0;
        private bool coordinatesLoaded = false;

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV 파일 (*.csv)|*.csv";
            openFileDialog.Title = "RA/Dec 좌표 파일 선택";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);

                    if (lines.Length < 2)
                    {
                        MessageBox.Show("CSV 파일에 유효한 데이터가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string[] values = lines[1].Split(',');

                    if (values.Length >= 2 &&
                        double.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out loadedRA) &&
                        double.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out loadedDec))
                    {
                        coordinatesLoaded = true;
                        MessageBox.Show($"RA/Dec 좌표 불러오기 성공:\n\nRA: {loadedRA}\nDec: {loadedDec}", "불러오기 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("RA/Dec 데이터를 올바르게 파싱할 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"CSV 파일 읽기 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (telescope == null || !telescope.Connected)
                {
                    MessageBox.Show("가대가 연결되지 않았습니다. 먼저 연결해 주세요.", "연결 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!coordinatesLoaded)
                {
                    MessageBox.Show("RA/Dec 좌표가 불러와지지 않았습니다. 먼저 좌표를 불러오세요.", "좌표 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 트래킹이 꺼져 있다면 켬
                if (!telescope.Tracking)
                {
                    telescope.Tracking = true;
                }

                telescope.SlewToCoordinates(loadedRA, loadedDec);
                MessageBox.Show($"가대를 아래 보정값으로 이동 완료했습니다:\n\nRA: {loadedRA}\nDec: {loadedDec}", "이동 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"가대 이동 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}