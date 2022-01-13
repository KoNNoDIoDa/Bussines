using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BusinessSim
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int ownersAm = 5; //Количество владельцев фирм
        private int workersAm = 18; // Общее количество рабочих
        private static int moneyAm = 2; //Количество монет на владельца
        private int bankReg; //Режим работы банка
        private int salary = 1; //Зарплата рабочего
        private int production = 2; //Количество продукта, производимого рабочим за день
        private bool ownersEat; //Ест ли владелец в рабочий день
        private int ownerNeedsFood = 1; //Количестов еды потребляемой хозяином
        private double percent = 0; //Ставка по кредиту в банке
        private DispatcherTimer timer = new DispatcherTimer();
        Firm[] ownerAr = new Firm[ownersAm]; //Массив Фирм
        Firm bank = new Firm(); //Банк
        int order = 0;
        bool working = true; //Работает ли хоть одна фирма
        bool shift = false; //Смена работы фирм (выходной / рабочий день)
        int neededWorkers = 0; //Количество нуждающихся рабочих
        int ownersDonate = 0;
        int sumOwnersMoney = ownersAm * moneyAm;
        int sumOwnersDuty = 0;
        int notWorking = 0;
        int activeWorkers = 0;
        int activeFirms = 0;
        int sumProduction = 0;

        public MainWindow()
        {
            InitializeComponent();
            
        }
        private void Regime0(object sender, RoutedEventArgs e)
        {
            bankReg = 2;
            bankMoney.IsEnabled = false;
            sumDuty.IsEnabled = false;
            sumDuty.Text = "";
            bankMoney.Text = "";
        }

        private void Regime1(object sender, RoutedEventArgs e)
        {
            bankReg = 0;
            if (bankMoney != null) bankMoney.IsEnabled = true;
            if (sumDuty != null) sumDuty.IsEnabled = true;
        }

        private void Regime2(object sender, RoutedEventArgs e)
        {
            bankReg = 1;
            bankMoney.IsEnabled = true;
            sumDuty.IsEnabled = true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            MainFunc();
        }
        public void DispatcherTimer_Tick(object sender, EventArgs e)
        {

            if (working)
            {
                for (int x = 0; x != ownerAr.Length; x++) //Цикл для перебора фирм на каждое действие
                {
                    
                    switch (order)
                    {
                        case 0:
                            if (ownerAr[x].works)
                            {
                                if (ownerAr[x].hungry != 4) //Определяет количество рабочих, которые поели //!!!!уровень голода 4 значит, что идёт первый день и ряд дейтсвий игнорируется!!!!!!
                                {


                                    int hung; //Количество рабочих, которым не хватило еды
                                    if ((shift && x % 2 != 0) || (!shift && x % 2 == 0))
                                    {
                                        hung = (ownerAr[x].workers <= ownerAr[x].goodsAm) ? 0 : (ownerAr[x].workers - ownerAr[x].goodsAm);

                                        if (bankReg != 1)
                                        {
                                            ownerAr[x].gave += (ownerAr[x].moneyAm - 1 >= 0) ? ownerAr[x].moneyAm - 1 : 0; //Высчитывает количество денег, которые могут быть выданы в долг
                                            ownersDonate += ownerAr[x].gave;
                                        }

                                        if (hung > 0)
                                        {
                                            activeWorkers -= ownerAr[x].workersAm;//bp
                                            ownerAr[x].workersAm = ownerAr[x].workers - hung;
                                            activeWorkers += ownerAr[x].workersAm;

                                            actWorkers.Text = Convert.ToString(activeWorkers);
                                        }
                                        else
                                        {
                                            sumProduction -= ownerAr[x].workers;
                                            ownerAr[x].goodsAm -= ownerAr[x].workers; //bp
                                            sumProd.Text = Convert.ToString(sumProduction);
                                        }
                                    }

                                }

                                //bank.gave += bank.moneyAm; //Высчитывает количество денег, которые могут быть выданы вдолг

                                if (ownerAr[x].moneyAm < ownerAr[x].workers * salary && (shift && x % 2 == 0) || (!shift && x % 2 != 0)) //Если количества денег недостаточно для выдачи зарплаты то добавляет 1 к количеству нуждающихся (толбко для фирм, у которых рабочий день)
                                {
                                    ownerAr[x].needMoney = true;
                                }
                                //Thread.Sleep(1000);
                            }

                            break;

                        case 1:
                            if (ownerAr[x].works)
                            {
                                if ((shift && x % 2 == 0) || (!shift && x % 2 != 0))
                                {
                                    ownerAr[x].goodsAm += ownerAr[x].workersAm * production; // Полученгие продукции от рабочих //bp
                                    sumProduction += ownerAr[x].workersAm * production;
                                    if (ownerAr[x].needMoney) //Выдача денег в долг
                                    {
                                        
                                        int given = ((ownerAr[x].workers * salary + ownerNeedsFood) - ownerAr[x].moneyAm > 0) ? (ownerAr[x].workers * salary - ownerAr[x].moneyAm) + ownerNeedsFood : 0; //Выданная сумма
                                        bank.moneyAm -= given;
                                        ownerAr[x].moneyAm += given;
                                        sumOwnersMoney += given;
                                        ownerAr[x].duty += given;
                                        sumOwnersDuty += ownerAr[x].duty;

                                        sumDuty.Text = (bankReg != 2) ? Convert.ToString(sumOwnersDuty) : "";

                                        if (given > ownersDonate)
                                        {
                                            ownersDonate = 0;
                                        }
                                        else ownersDonate -= given;

                                        bankMoney.Text = (bankReg != 2) ? Convert.ToString(bank.moneyAm) : "";
                                    }
                                    ownerAr[x].totalMoneyWorkers += (ownerAr[x].moneyAm > ownerAr[x].workers) ? (ownerAr[x].moneyAm > ownerAr[x].workers * salary) ? ownerAr[x].workers * salary : ownerAr[x].moneyAm : ownerAr[x].moneyAm; //Выдача зарплаты //bp
                                    sumOwnersMoney -= (ownerAr[x].moneyAm > ownerAr[x].workers) ? (ownerAr[x].moneyAm > ownerAr[x].workers * salary) ? ownerAr[x].workers * salary : ownerAr[x].moneyAm : ownerAr[x].moneyAm;
                                    ownerAr[x].moneyAm -= (ownerAr[x].moneyAm > ownerAr[x].workers) ? (ownerAr[x].moneyAm > ownerAr[x].workers * salary) ? ownerAr[x].workers * salary : ownerAr[x].moneyAm : ownerAr[x].moneyAm;
                                    

                                    ownersMoney.Text = Convert.ToString(sumOwnersMoney);

                                    if (ownersEat) //Владелец употребляет продукт
                                    {
                                        ownerAr[x].hungry = (ownerAr[x].goodsAm >= ownerNeedsFood) ? 0 : ownerAr[x].hungry + 1;
                                        sumProduction -= (ownerAr[x].goodsAm >= ownerNeedsFood) ? ownerNeedsFood : ownerAr[x].goodsAm;
                                        ownerAr[x].goodsAm -= (ownerAr[x].goodsAm >= ownerNeedsFood) ? ownerNeedsFood : ownerAr[x].goodsAm; //bp
                                    }

                                    ownerAr[x].gaveGoods = (ownerAr[x].goodsAm > ownerNeedsFood + ownerAr[x].workers) ? ownerAr[x].goodsAm - ownerNeedsFood - ownerAr[x].workers : 0; //Сколько продукции может продать каждый владелец
                                    bank.goodsAm += ownerAr[x].gaveGoods; //Общее количество продукта на продажу
                                }

                                neededWorkers += (ownerAr[x].goodsAm <= ownerAr[x].workers) ? ownerAr[x].workers - ownerAr[x].goodsAm : 0; //Количество работников, которым придётся брать продукт у других фирм
                                sumProd.Text = Convert.ToString(sumProduction);
                            }
                            break;

                        case 2:
                            if (ownerAr[x].works)
                            {
                                if (ownerAr[x].hungry == 4) ownerAr[x].hungry = 0; //Выход с 1 цикла, перевод компаний в обычный режим

                                if ((shift && x % 2 != 0) || (!shift && x % 2 == 0))
                                {
                                    if (ownerAr[x].goodsAm < ownerNeedsFood && bank.goodsAm > ownerNeedsFood && ownerAr[x].moneyAm >= ownerNeedsFood) //Покупка еды для владельца
                                    {
                                        sumOwnersMoney -= ownerNeedsFood;
                                        ownerAr[x].moneyAm -= ownerNeedsFood;
                                        bank.goodsAm -= (bank.goodsAm >= ownerNeedsFood) ? ownerNeedsFood : bank.goodsAm;
                                        ownerAr[x].hungry = 0;
                                    }
                                    else if (ownerAr[x].moneyAm <= ownerNeedsFood) ownerAr[x].hungry += 1; //Повышения индекса голода в случае недостатка денег

                                    ownersMoney.Text = Convert.ToString(sumOwnersMoney);

                                }
                                if ((shift && x % 2 == 0) || (!shift && x % 2 != 0))
                                {
                                    activeWorkers -= ownerAr[x].workersAm;
                                    ownerAr[x].workersAm = (ownerAr[x].totalMoneyWorkers >= ownerAr[x].workers) ? ownerAr[x].workers : (ownerAr[x].totalMoneyWorkers > 0) ? (ownerAr[x].workers <= ownerAr[x].totalMoneyWorkers) ? ownerAr[x].totalMoneyWorkers - ownerAr[x].workers : ownerAr[x].totalMoneyWorkers : 0; //Расчёт активных рвбочих
                                    activeWorkers += ownerAr[x].workersAm;

                                    actWorkers.Text = Convert.ToString(activeWorkers);

                                    if (ownerAr[x].goodsAm >= ownerAr[x].workers) //Покупка продукции у хозяина
                                    {
                                        sumProduction -= (ownerAr[x].totalMoneyWorkers >= ownerAr[x].workers) ? ownerAr[x].workers : (ownerAr[x].goodsAm >= ownerAr[x].workersAm) ? ownerAr[x].workersAm : ownerAr[x].goodsAm;
                                        ownerAr[x].moneyAm += (ownerAr[x].totalMoneyWorkers >= ownerAr[x].workers) ? ownerAr[x].workers : (ownerAr[x].goodsAm >= ownerAr[x].workersAm) ? ownerAr[x].workersAm : ownerAr[x].goodsAm;
                                        sumOwnersMoney += (ownerAr[x].totalMoneyWorkers >= ownerAr[x].workers) ? ownerAr[x].workers : (ownerAr[x].goodsAm >= ownerAr[x].workersAm) ? ownerAr[x].workersAm : ownerAr[x].goodsAm;
                                        ownerAr[x].goodsAm -= (ownerAr[x].totalMoneyWorkers >= ownerAr[x].workers) ? ownerAr[x].workers : (ownerAr[x].goodsAm >= ownerAr[x].workersAm) ? ownerAr[x].workersAm : ownerAr[x].goodsAm;
                                    }
                                    else //Покупка продукции у других фирм
                                    {
                                        if (ownerAr[x].goodsAm > 0 && bank.goodsAm > 0) //Если у хозяина есть продукция
                                        {
                                            bank.goodsAm -= (ownerAr[x].workersAm - ownerAr[x].goodsAm < bank.goodsAm / neededWorkers) ? ownerAr[x].workersAm - ownerAr[x].goodsAm : bank.goodsAm / neededWorkers;
                                            sumProduction -= ownerAr[x].goodsAm;
                                            ownerAr[x].moneyAm += ownerAr[x].goodsAm;
                                            sumOwnersMoney += ownerAr[x].goodsAm;
                                            ownerAr[x].goodsAm = 0;
                                        }
                                        else if (bank.goodsAm > 0 && neededWorkers != 0) bank.goodsAm -= (ownerAr[x].workersAm < bank.goodsAm / neededWorkers) ? ownerAr[x].workersAm : bank.goodsAm / neededWorkers; //Если у хозяина продукции нет
                                        else if (neededWorkers == 0) bank.goodsAm = 0;
                                    }
                                    ownerAr[x].totalMoneyWorkers -= (ownerAr[x].totalMoneyWorkers >= ownerAr[x].workersAm) ? ownerAr[x].workersAm : ownerAr[x].totalMoneyWorkers; //Расчёт денег на всех рабочих //bp


                                }
                                if (ownerAr[x].workersAm <= 0 && ownerAr[x].works)
                                {
                                    notWorking++;
                                    ownerAr[x].works = false; //bp
                                    sumProduction -= ownerAr[x].goodsAm;
                                    sumOwnersMoney -= ownerAr[x].moneyAm;
                                    activeWorkers -= ownerAr[x].workersAm;
                                    activeFirms--;

                                    actFirms.Text = Convert.ToString(activeFirms);
                                    actWorkers.Text = Convert.ToString(activeWorkers);
                                }

                                sumProd.Text = Convert.ToString(sumProduction);
                                /*
                                 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!РАСЧЁТ ДОЛГОВ И ПЛАТА ЗА ПРОДУКЦИЮ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                 */
                                if (bank.goodsAm < ownerAr[x].gaveGoods && bank.goodsAm > 0 && ownerAr[x].gaveGoods > 0)
                                {
                                    int foodGiven = ownerAr[x].gaveGoods - bank.goodsAm;
                                    sumProduction -= foodGiven;
                                    ownerAr[x].goodsAm -= foodGiven;
                                    ownerAr[x].moneyAm += foodGiven;
                                    sumOwnersMoney += foodGiven;
                                    ownerAr[x].gaveGoods = 0;
                                }
                                else if (bank.goodsAm > 0 && ownerAr[x].gaveGoods > 0)
                                {
                                    bank.goodsAm -= ownerAr[x].gaveGoods;
                                    ownerAr[x].gaveGoods = 0;
                                }

                                if (sumOwnersDuty > ownersDonate || bankReg == 1)
                                {
                                    ownerAr[x].dutyPercent += (sumOwnersDuty - ownersDonate > ownerAr[x].duty) ? ownerAr[x].duty / 100 * percent : sumOwnersDuty / 100 * percent;
                                    while (ownerAr[x].dutyPercent >= 1)
                                    {
                                        ownerAr[x].dutyPercent--;
                                        ownerAr[x].duty++;
                                        sumOwnersDuty++;
                                    }

                                    sumOwnersDuty -= (ownerAr[x].moneyAm > ownerAr[x].duty) ? ownerAr[x].duty : ownerAr[x].moneyAm;
                                    bank.moneyAm += (ownerAr[x].moneyAm > ownerAr[x].duty) ? ownerAr[x].duty : ownerAr[x].moneyAm;
                                    ownerAr[x].duty -= (ownerAr[x].moneyAm > ownerAr[x].duty) ? ownerAr[x].duty : ownerAr[x].moneyAm;
                                    bankMoney.Text = (bankReg != 2) ? Convert.ToString(bank.moneyAm) : "";
                                    sumDuty.Text = (bankReg != 2) ? Convert.ToString(sumOwnersDuty) : "";
                                }

                                if (bankReg == 1)
                                {
                                    if (ownerAr[x].duty <= ownerAr[x].moneyAm)
                                    {
                                        sumOwnersDuty -= ownerAr[x].duty;
                                        bank.moneyAm += ownerAr[x].duty;
                                        ownerAr[x].moneyAm -= ownerAr[x].duty;
                                        sumOwnersMoney -= ownerAr[x].duty;
                                        ownerAr[x].duty = 0;
                                        

                                    }
                                }
                                else if (ownerAr[x].moneyAm > 0 && (ownerAr[x].gave > 0 || ownerAr[x].duty > 0))
                                {
                                    //ownerAr[x].moneyAm += ownerAr[x].gave;
                                    ownerAr[x].gave = 0;

                                    if (ownerAr[x].duty > 0 && ownerAr[x].duty <= ownerAr[x].moneyAm)
                                    {
                                        ownerAr[x].moneyAm -= ownerAr[x].duty;
                                        sumOwnersMoney -= ownerAr[x].duty;
                                        sumOwnersDuty -= ownerAr[x].duty;
                                        bank.moneyAm += ownerAr[x].duty;

                                        ownerAr[x].duty = 0;
                                        
                                    }
                                }

                                if (ownerAr[x].duty > 0 && ownerAr[x].works)
                                {
                                    ownerAr[x].works = false; //bp
                                    sumProduction -= ownerAr[x].goodsAm;
                                    sumOwnersMoney -= ownerAr[x].moneyAm;
                                    sumOwnersDuty -= ownerAr[x].duty;
                                    notWorking++;
                                    activeWorkers -= ownerAr[x].workersAm;
                                    activeFirms--;

                                    actFirms.Text = Convert.ToString(activeFirms);
                                    actWorkers.Text = Convert.ToString(activeWorkers);
                                }


                                ownersMoney.Text = Convert.ToString(sumOwnersMoney);
                                sumDuty.Text = (bankReg != 2) ? Convert.ToString(sumOwnersDuty) : "";
                                //Thread.Sleep(1000);
                                sumProd.Text = Convert.ToString(sumProduction);
                            }

                            break;



                    }

                    if (notWorking >= ownerAr.Length)
                    {
                        working = false;
                    }

                    if (order >= 2) shift = !shift; //Смена порядка рабочих дней и выходных
                    if (x == ownerAr.Length - 1) order += (order != 2) ? 1 : -order;

                }
                
            }
            else
            {
                TurnTheLight(true);
                timer.Stop();
            }
        }
        public void MainFunc()
        {
            timer.Tick += DispatcherTimer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 50);

            ownersEat = Convert.ToBoolean(ownerEatsAtWork.IsChecked);
            ownersAm = Convert.ToInt32(startOwners.Text);
            workersAm = Convert.ToInt32(startWorkers.Text);
            production = Convert.ToInt32(prodAm.Text);
            moneyAm = Convert.ToInt32(startCoins.Text);
            ownerNeedsFood = Convert.ToInt32(ownerNeeds.Text);
            salary = Convert.ToInt32(workersSalary.Text);
            percent = Convert.ToInt32(perCent.Text);
            ownerAr = new Firm[ownersAm];

            bank = new Firm();
            order = 0;
            working = true;
            shift = false;
            neededWorkers = 0;
            ownersDonate = 0;
            sumOwnersMoney = ownersAm * moneyAm;
            sumOwnersDuty = 0;
            notWorking = 0;
            activeWorkers = 0;
            activeFirms = 0;
            sumProduction = 0;

            for (int i = 0; i < ownersAm; i++)
            {
                ownerAr[i] = new Firm();
            }


                ownersMoney.Text = Convert.ToString(ownersAm * moneyAm);

            sumDuty.Text = (bankReg != 2) ? "0" : "";

            if (bankReg < 2)
            {
                if (Convert.ToBoolean(ownersStartWithMoney.IsChecked)) moneyAm = 0;
                    bankMoney.Text = (bankReg != 2) ? Convert.ToString(bank.moneyAm) : "";
            }
            else percent = 0;
            int workersPerOwner = workersAm % ownerAr.Length; //Определяет остаток рабочих после распределения
            for(int i = 0; i < ownerAr.Length; i++) //распределяет рабочих по фирмам
            {  
                for(int y = 0; y < workersAm / ownerAr.Length; y++)
                {
                    ownerAr[i].workers++;
                }
                if (Convert.ToBoolean(!ownersStartWithMoney.IsChecked)) ownerAr[i].moneyAm = moneyAm; //если банк не работает, то деньги находятся у фирм с самого начала
                if (workersPerOwner > 0) //Распределяет остаток рабочих по фирмам
                {
                    ownerAr[i].workers++;
                    workersPerOwner--;
                }
                ownerAr[i].workersAm = ownerAr[i].workers;
            }

            if (Convert.ToBoolean(ownersStartWithMoney.IsChecked))
            {
                bank.moneyAm = sumOwnersMoney;
                sumOwnersMoney = 0;
            }

            activeWorkers = workersAm;
            activeFirms = ownerAr.Length;
            actWorkers.Text = Convert.ToString(workersAm);
            actFirms.Text = Convert.ToString(activeFirms);
            sumProd.Text = Convert.ToString(sumProduction);

            TurnTheLight(false);
            
            timer.Start();


        }

        public void TurnTheLight(bool turnOn)
        {
            firstColumn.Background = (!turnOn) ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(Colors.White);
            firstColumn.IsEnabled = turnOn;

            secondColumn.Background = (turnOn) ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(Colors.White);
            secondColumn.IsEnabled = !turnOn;
        }

        
    }

    public class Firm //Набор параметров каждой фрмы
    {
        public int workers; //Количестов рабочих
        public int moneyAm; //Количество денег
        public int goodsAm; //Колличество продукта
        public double dutyPercent; //Процент по задолжностям
        public int duty; //Долг
        public int gave; //Количество денег, данное вдолг
        public int hungry = 4; //Индекс голода
        public int workersAm; //Количестов активных рабочих
        public int totalMoneyWorkers; //Общая сумма денег у рабочих
        public bool needMoney; //Недостаток денег
        public int gaveGoods; //Количество проданной продукции
        public bool works = true; //Статус работы фирмы
    }
}
  