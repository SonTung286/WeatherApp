using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace WeatherApp
{
    public partial class MainForm : Form
    {
        private readonly string apiKey = "3288ec8217e35735351a02c9be62aa06"; // API Key

        private TextBox txtCitySearch;
        private Button btnSearch;
        private Button btnAddToFavorites;
        private ListBox lstFavoriteCities;
        private Panel mainWeatherPanel;
        private Label lblCity;
        private Label lblTemperature;
        private Label lblWeather;
        private PictureBox weatherIcon;
        private Panel[] detailPanels;
        private Label lblHumidity, lblWindSpeed, lblPressure, lblUVIndex;
        private Panel forecastPanel;
        private Label lblForecastHeader;
        private Label[] lblForecastDays;
        private Label[] lblForecastTemps;

        public MainForm()
        {
            // Cài đặt giao diện chính
            this.Text = "Weather App";
            this.Size = new System.Drawing.Size(1000, 600);
            this.BackColor = ColorTranslator.FromHtml("#f0f4f8");

            // Panel bên trái cho tìm kiếm và danh sách yêu thích
            Panel leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(leftPanel);

            txtCitySearch = new TextBox
            {
                Location = new Point(10, 10),
                Width = 180,
                PlaceholderText = "Search city...",
                Font = new Font("Arial", 10),
            };
            leftPanel.Controls.Add(txtCitySearch);

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(10, 40),
                Width = 180
            };
            btnSearch.Click += new EventHandler(BtnSearch_Click);
            leftPanel.Controls.Add(btnSearch);

            // Thêm nút "Add to Favorites"
            btnAddToFavorites = new Button
            {
                Text = "Add to Favorites",
                Location = new Point(10, 70),
                Width = 180
            };
            btnAddToFavorites.Click += new EventHandler(BtnAddToFavorites_Click);
            leftPanel.Controls.Add(btnAddToFavorites);

            lstFavoriteCities = new ListBox
            {
                Location = new Point(10, 110),
                Size = new Size(180, 350),
                Font = new Font("Arial", 12),
                Items = { "Hà Nội", "Thành phố Hồ Chí Minh", "Đà Nẵng" }
            };
            lstFavoriteCities.SelectedIndexChanged += (sender, e) => txtCitySearch.Text = lstFavoriteCities.SelectedItem.ToString();
            leftPanel.Controls.Add(lstFavoriteCities);

            // Panel hiển thị thông tin thời tiết chính
            mainWeatherPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(475, 220),
                Location = new Point(220, 0),
                BackColor = ColorTranslator.FromHtml("#87CEFA"),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(mainWeatherPanel);

            lblCity = new Label
            {
                Text = "City",
                Location = new Point(20, 10),
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            mainWeatherPanel.Controls.Add(lblCity);

            lblTemperature = new Label
            {
                Text = "Temperature",
                Location = new Point(20, 50),
                Font = new Font("Arial", 36, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            mainWeatherPanel.Controls.Add(lblTemperature);

            lblWeather = new Label
            {
                Text = "Weather",
                Location = new Point(20, 120),
                Font = new Font("Arial", 18, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true
            };
            mainWeatherPanel.Controls.Add(lblWeather);

            // Thêm PictureBox để hiển thị biểu tượng thời tiết
            weatherIcon = new PictureBox
            {
                Size = new Size(150, 150),
                Location = new Point(300, 50),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = ColorTranslator.FromHtml("#87CEFA")
            };
            mainWeatherPanel.Controls.Add(weatherIcon);

            // Tạo các ô cho thông tin chi tiết và đặt lại vị trí
            detailPanels = new Panel[4];
            lblHumidity = CreateDetailPanel("Humidity", new Point(220, 240), out detailPanels[0]);
            lblWindSpeed = CreateDetailPanel("Wind Speed", new Point(370, 240), out detailPanels[1]);
            lblPressure = CreateDetailPanel("Pressure", new Point(220, 320), out detailPanels[2]);
            lblUVIndex = CreateDetailPanel("UV Index", new Point(370, 320), out detailPanels[3]);

            // Điều chỉnh vị trí và kích thước của các panel thuộc tính để thẳng hàng với mainWeatherPanel
            int detailPanelStartX = mainWeatherPanel.Left;
            detailPanels[0].Location = new Point(detailPanelStartX, 240);
            detailPanels[1].Location = new Point(detailPanelStartX + 250, 240);
            detailPanels[2].Location = new Point(detailPanelStartX, 320);
            detailPanels[3].Location = new Point(detailPanelStartX + 250, 320);

            foreach (var panel in detailPanels)
            {
                panel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }
            this.Controls.AddRange(detailPanels);

            // Panel bên phải cho dự báo 7 ngày
            forecastPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 270, // Tăng độ rộng của forecastPanel
                Height = 700,
                Padding = new Padding(10, 0, 10, 0), // Tạo khoảng cách với mép phải
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(this.ClientSize.Width - 270, 0), // Cách mép phải 20px
                Anchor = AnchorStyles.Top | AnchorStyles.Right // Cố định vị trí ở bên phải và trên cùng khi thay đổi kích thước cửa sổ
            };
            this.Controls.Add(forecastPanel);

            lblForecastHeader = new Label
            {
                Text = "7-Day Forecast",
                Location = new Point(10, 10),
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true
            };
            forecastPanel.Controls.Add(lblForecastHeader);

            int dayStartY = lblForecastHeader.Bottom + 20;

            // Cập nhật tên ngày trong tuần
            lblForecastDays = new Label[7];
            lblForecastTemps = new Label[7];
            for (int i = 0; i < 7; i++)
            {
                lblForecastDays[i] = new Label
                {
                    Text = DateTime.Now.AddDays(i).ToString("dddd"),
                    Location = new Point(10, dayStartY + (i * 50)),
                    Font = new Font("Arial", 12),
                    AutoSize = true
                };
                lblForecastTemps[i] = new Label
                {
                    Text = "High / Low",
                    Location = new Point(120, dayStartY + (i * 50)),
                    Font = new Font("Arial", 12),
                    AutoSize = true
                };
                forecastPanel.Controls.Add(lblForecastDays[i]);
                forecastPanel.Controls.Add(lblForecastTemps[i]);
            }
        }

        private Label CreateDetailPanel(string title, Point location, out Panel panel)
        {
            panel = new Panel
            {
                Size = new Size(225, 70),
                Location = location,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left // Giữ panel ở vị trí cố định khi phóng to
            };

            Label lblTitle = new Label
            {
                Text = title,
                Location = new Point(10, 10),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            Label lblValue = new Label
            {
                Text = "N/A",
                Location = new Point(10, 30),
                Font = new Font("Arial", 12, FontStyle.Regular),
                AutoSize = true
            };
            panel.Controls.Add(lblValue);

            this.Controls.Add(panel);
            return lblValue;
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            string city = txtCitySearch.Text;
            if (string.IsNullOrWhiteSpace(city))
            {
                MessageBox.Show("Please enter a city name.");
                return;
            }

            await GetWeatherAsync(city);
        }

        private void BtnAddToFavorites_Click(object sender, EventArgs e)
        {
            string city = txtCitySearch.Text;
            if (string.IsNullOrWhiteSpace(city))
            {
                MessageBox.Show("Please search for a city first.");
                return;
            }

            // Kiểm tra xem thành phố đã có trong danh sách chưa
            bool cityExists = false;
            foreach (var item in lstFavoriteCities.Items)
            {
                if (item.ToString().Equals(city, StringComparison.OrdinalIgnoreCase))
                {
                    cityExists = true;
                    break;
                }
            }

            if (!cityExists)
            {
                lstFavoriteCities.Items.Add(city); // Thêm vào danh sách nếu chưa có
            }
            else
            {
                MessageBox.Show("City is already in the favorites list.");
            }
        }

        private async Task GetWeatherAsync(string city)
        {
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            string forecastUrl = $"http://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Lấy thông tin thời tiết hiện tại
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseBody);

                    lblCity.Text = city;
                    lblTemperature.Text = $"{json["main"]?["temp"]?.ToString() ?? "N/A"}°C";

                    string weatherDescription = json["weather"]?[0]?["description"]?.ToString() ?? "N/A";
                    lblWeather.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(weatherDescription);

                    lblHumidity.Text = $"{json["main"]?["humidity"]?.ToString() ?? "N/A"}%";
                    lblWindSpeed.Text = $"{json["wind"]?["speed"]?.ToString() ?? "N/A"} km/h";
                    lblPressure.Text = $"{json["main"]?["pressure"]?.ToString() ?? "N/A"} hPa";

                    // Lấy tọa độ thành phố và gọi hàm lấy UV Index
                    double lat = json["coord"]?["lat"]?.ToObject<double>() ?? 0;
                    double lon = json["coord"]?["lon"]?.ToObject<double>() ?? 0;
                    await GetUVIndexAsync(lat, lon);

                    // Cập nhật biểu tượng thời tiết
                    string iconCode = json["weather"]?[0]?["icon"]?.ToString();
                    if (!string.IsNullOrEmpty(iconCode))
                    {
                        string iconUrl = $"http://openweathermap.org/img/wn/{iconCode}@2x.png";
                        weatherIcon.Load(iconUrl); 
                    }       

                    // Lấy dữ liệu dự báo thời tiết 5 ngày
                    HttpResponseMessage forecastResponse = await client.GetAsync(forecastUrl);
                    forecastResponse.EnsureSuccessStatusCode();
                    string forecastBody = await forecastResponse.Content.ReadAsStringAsync();
                    JObject forecastJson = JObject.Parse(forecastBody);

                    // Cập nhật dự báo 7 ngày
                    var forecastList = forecastJson["list"]?.ToObject<JArray>();
                    for (int i = 0; i < 7; i++)
                    {
                        int forecastIndex = i * 8;

                        if (forecastList != null && forecastList.Count > forecastIndex)
                        {
                            // Lấy dữ liệu có sẵn cho các ngày từ 1 đến 5
                            string dateText = forecastList[forecastIndex]?["dt_txt"]?.ToString() ?? "N/A";
                            double tempMin = forecastList[forecastIndex]?["main"]?["temp_min"]?.ToObject<double>() ?? 0;
                            double tempMax = forecastList[forecastIndex]?["main"]?["temp_max"]?.ToObject<double>() ?? 0;

                            lblForecastDays[i].Text = DateTime.Parse(dateText).ToString("dddd");
                            lblForecastTemps[i].Text = $"{tempMax}°C / {tempMin}°C";
                        }
                        else
                        {
                            // Dự đoán dữ liệu cho ngày thứ 6 và thứ 7
                            double avgTempMin = 0, avgTempMax = 0;
                            int count = Math.Min(i, 5);

                            for (int j = 0; j < count; j++)
                            {
                                int previousIndex = j * 8;
                                avgTempMin += forecastList?[previousIndex]?["main"]?["temp_min"]?.ToObject<double>() ?? 0;
                                avgTempMax += forecastList?[previousIndex]?["main"]?["temp_max"]?.ToObject<double>() ?? 0;
                            }

                            avgTempMin /= count;
                            avgTempMax /= count;

                            // Tính toán ngày cụ thể cho ngày 6 và ngày 7 dựa trên ngày cuối cùng có trong API
                            DateTime lastDate = DateTime.Parse(forecastList[4 * 8]?["dt_txt"]?.ToString() ?? DateTime.Now.ToString());
                            DateTime calculatedDate = lastDate.AddDays(i - 4);

                            lblForecastDays[i].Text = calculatedDate.ToString("dddd");
                            lblForecastTemps[i].Text = $"{avgTempMax:F1}°C / {avgTempMin:F1}°C";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        private async Task GetUVIndexAsync(double lat, double lon)
        {
            string uvUrl = $"http://api.openweathermap.org/data/2.5/uvi?appid={apiKey}&lat={lat}&lon={lon}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(uvUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseBody);

                    lblUVIndex.Text = $"{json["value"]?.ToString() ?? "N/A"}";
                }
            }
            catch (Exception ex)
            {
                lblUVIndex.Text = "N/A";
                MessageBox.Show($"Error retrieving UV index: {ex.Message}");
            }
        }
    }
}
