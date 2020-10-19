using Atomus.Page.Menu.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Atomus.Page.Menu.ViewModel
{
    public class ModernMenuViewModel : MVVM.ViewModel
    {
        #region Declare
        private readonly string Configfilename;

        private string nickName;
        private string info1;
        private string info2;
        private string info3;
        private double levelRate;
        private MenuItem selectedMenuItem;

        ObservableCollection<Exchange> exchanges;
        private Exchange selectedExchange;
        #endregion

        #region Property
        public ICore Core { get; set; }

        public ICommand MenuItemSelectedCommand { get; set; }
        public ICommand SettingCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public string NickName
        {
            get
            {
                return this.nickName;
            }
            set
            {
                if (this.nickName != value)
                {
                    this.nickName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Info1
        {
            get
            {
                if (this.info1.IsNullOrEmpty() && this.info2.IsNullOrEmpty() && this.info3.IsNullOrEmpty())
                    this.GetPoint();

                return this.info1;
            }
            set
            {
                if (this.info1 != value)
                {
                    this.info1 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Info2
        {
            get
            {
                if (this.info1.IsNullOrEmpty() && this.info2.IsNullOrEmpty() && this.info3.IsNullOrEmpty())
                    this.GetPoint();

                return this.info2;
            }
            set
            {
                if (this.info2 != value)
                {
                    this.info2 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Info3
        {
            get
            {
                if (this.info1.IsNullOrEmpty() && this.info2.IsNullOrEmpty() && this.info3.IsNullOrEmpty())
                    this.GetPoint();

                return this.info3;
            }
            set
            {
                if (this.info3 != value)
                {
                    this.info3 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Progress
        {
            get
            {
                if (this.info1.IsNullOrEmpty() && this.info2.IsNullOrEmpty() && this.info3.IsNullOrEmpty())
                    this.GetPoint();

                if (this.levelRate <= 0)
                    return 0;
                else
                    return this.levelRate / 100;
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<MenuItem> MenuItems { get; set; }
        public MenuItem SelectedMenuItem
        {
            get
            {
                return this.selectedMenuItem;
            }
            set
            {
                if (this.selectedMenuItem != value)
                {
                    this.selectedMenuItem = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<Exchange> Exchanges
        {
            get
            {
                return this.exchanges;
            }
            set
            {
                if (this.exchanges != value)
                {
                    this.exchanges = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Exchange SelectedExchange
        {
            get
            {
                return this.selectedExchange;
            }
            set
            {
                if (this.selectedExchange != value)
                {
                    if (this.selectedExchange != null && value != null)
                        this.Save(value.ExchangeID);

                    this.selectedExchange = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        #region INIT
        public ModernMenuViewModel()
        {
            this.Configfilename = Path.Combine(Factory.FolderPath, $"DefaultLoginStandard.config");

            this.nickName = (string)Config.Client.GetAttribute("Account.NICKNAME");

            this.MenuItems = new ObservableCollection<MenuItem>();

            //this.MenuItemSelectedCommand = new Command(async () => await this.MenuItemSelected());
            this.LogoutCommand = new Command(async () => await this.LogoutProcess()
                                            , () => { return true; });
        }

        public ModernMenuViewModel(ICore core) : this()
        {
            IAction _Core;

            this.Core = core;

            _Core = (IAction)Config.Client.GetAttribute("DebugPage");

            if (_Core != null)
                this.LoadMenu(-1, -1, _Core.GetAttributeDecimal("ASSEMBLY_ID"));
            else
                this.LoadMenu(-1, -1, -1);
        }

        #endregion

        #region IO
        private async void LoadMenu(decimal START_MENU_ID, decimal ONLY_PARENT_MENU_ID, decimal ASSEMBLY_ID)
        {
            Service.IResponse result;

            try
            {
                result = await this.Core.SearchAsync(START_MENU_ID, ONLY_PARENT_MENU_ID, ASSEMBLY_ID);

                if (result.Status == Service.Status.OK)
                    foreach (DataRow dataRow in result.DataSet.Tables[1].Rows)
                    {
                        this.MenuItems.Add(new MenuItem()
                        {
                            MenuID = (decimal)dataRow["MENU_ID"],
                            AssemblyID = dataRow["ASSEMBLY_ID"] != DBNull.Value ? (decimal)dataRow["ASSEMBLY_ID"] : -1,
                            VisibleOne = dataRow["VISIBLE_ONE"] != DBNull.Value ? ((string)dataRow["VISIBLE_ONE"]) == "Y" : true,
                            Title = (string)dataRow["NAME"],
                            Image = (string)dataRow["IMAGE_URL1"],
                            BackgroundColor = dataRow["ASSEMBLY_ID"] != DBNull.Value ? Color.Transparent : "#16558bdc".ToColor()
                        });
                    }
                else
                    await Application.Current.MainPage.DisplayAlert("Warning", result.Message, "OK");
            }
            finally
            {
            }
        }

        public async void GetPoint()
        {
            Service.IResponse result;
            ObservableCollection<Exchange> list;

            try
            {
                result = await this.Core.SearchInfoAsync();

                if (result.Status == Service.Status.OK && result.DataSet != null && result.DataSet.Tables.Count > 0)
                {
                    foreach (DataRow dataRow in result.DataSet.Tables[0].Rows)
                    {
                        this.Info1 = (string)dataRow["INFO1"];
                        this.Info2 = (string)dataRow["INFO2"];
                        this.Info3 = (string)dataRow["INFO3"];
                        this.levelRate = (double)(decimal)dataRow["LEVEL_RATE"];
                        this.Progress = 0;
                    }

                    if ((int)result.DataSet.Tables[0].Rows[0]["LEVEL_UP_COUNT"] > 0)
                    {
                        try
                        {
                            // Use default vibration length
                            Vibration.Vibrate();

                            // Or use specified time
                            //var duration = TimeSpan.FromSeconds(1);
                            Vibration.Vibrate();
                        }
                        catch (FeatureNotSupportedException ex)
                        {
                            // Feature not supported on device
                        }
                        catch (Exception ex)
                        {
                            // Other error has occurred.
                        }

                        await Application.Current.MainPage.DisplayAlert("Level Up !!", "포인트 차감률이 낮아 졌습니다.", "OK");
                    }

                    if (result.DataSet.Tables.Count > 1 && exchanges == null)
                    {
                        list = new ObservableCollection<Exchange>();
                        foreach (DataRow dataRow in result.DataSet.Tables[1].Rows)
                        {
                            list.Add(new Exchange()
                            {
                                ExchangeID = (decimal)dataRow["EXCHANGE_ID"],
                                ExchangeName = (string)dataRow["EXCHANGE_NAME"],
                            });
                        }

                        this.Exchanges = list;
                    }

                    if (this.exchanges != null && this.exchanges.Count > 0)
                    {
                        foreach (Exchange exchange in this.exchanges)
                        {
                            if (exchange.ExchangeID == (decimal)result.DataSet.Tables[0].Rows[0]["EXCHANGE_ID"])
                            {
                                this.SelectedExchange = exchange;
                                break;
                            }
                        }

                        Config.Client.SetAttribute("Account.CURRENT_EXCHANGE_ID", (decimal)result.DataSet.Tables[0].Rows[0]["EXCHANGE_ID"]);
                    }
                }
                else
                    await Application.Current.MainPage.DisplayAlert("Warning", result.Message, "OK");
            }
            finally
            {
            }
        }

        private async void Save(decimal EXCHANGE_ID)
        {
            Service.IResponse result;

            try
            {
                result = await this.Core.SaveAsync(EXCHANGE_ID);

                if (result.Status == Service.Status.OK)
                {
                    if (Application.Current.MainPage is MasterDetailPage)
                    {
                        await ((Application.Current.MainPage as MasterDetailPage).Detail as NavigationPage).PopToRootAsync();
                        (Application.Current.MainPage as MasterDetailPage).IsPresented = false;
                    }

                    Config.Client.SetAttribute("Account.CURRENT_EXCHANGE_ID", EXCHANGE_ID);
                }
                else
                    await Application.Current.MainPage.DisplayAlert("Warning", result.Message, "OK");
            }
            finally
            {
            }
        }

        private async Task LogoutProcess()
        {
            bool result;

            try
            {
                (this.LogoutCommand as Command).ChangeCanExecute();

                result = await Application.Current.MainPage.DisplayAlert("로그아웃", "로그아웃하시겠습니까??", "예", "아니요");

                if (result)
                {

                    if (File.Exists(this.Configfilename))
                        File.Delete(this.Configfilename);

                    DependencyService.Get<INativeHelper>().CloseApp();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Warning", ex.Message, "OK");
            }
            finally
            {
                (this.LogoutCommand as Command).ChangeCanExecute();
            }
        }
        #endregion

        #region ETC
        #endregion
    }
}
