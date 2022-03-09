using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConvertExpressionLib;

namespace DistributionLib
{
    public class Analog1
    {
        public DataTable InDT { get; set; }
        public DataTable OutDT { get; set; } = new DataTable("Output data table");
        public Dictionary<String, int> colDictionary { get; set; }
        public List<string> exceptionList { get; set; } = new List<string>{ };

        public void TraceExceptionList()
		{
            Trace.WriteLine("---------------------------------");
            Trace.WriteLine("exceptionList.Count() = " + exceptionList.Count());
            if (exceptionList.Count() > 0)
                foreach(string str in exceptionList)
			    {
                    Trace.WriteLine(str);
			    }
		}

        public int GetCol(string key)
		{
            int value;
            if (colDictionary.TryGetValue(key, out value))
			{
                return value;
			}
            else
			{
                Trace.WriteLine("В списке столбцов нет " + key);
                exceptionList.Add("В списке столбцов нет " + key);
                return 0;
			}
		
        
        
        }

        public DateTime tryConvertToDateTime(string str)
		{
            Trace.WriteLine("str = " + str);
            DateTime dateTime = new DateTime();
            if (!String.IsNullOrEmpty(str))
            {
                dateTime = Convert.ToDateTime(str);
            }
            return dateTime;
        }
        
        //Упрощение обращения к функции во время разработри
        //TODO Надо избавиться от этого
        public DateTime ConDT(string str)
		{
            return tryConvertToDateTime(str);
		}

        bool IsFree(int idEquipment, DateTime startDT, DateTime endDT)
        {
            for(int j = 0; j < InDT.Rows.Count; j++)
            {
                Trace.WriteLine("startDT = " + startDT);
                Trace.WriteLine("endDT = " + endDT);
                Trace.WriteLine("Оборудование совпадает = " + (idEquipment == Convert.ToInt32(InDT.Rows[j].ItemArray[GetCol("IdEquipment")].ToString())));
                string oldStartDTStr = InDT.Rows[j].ItemArray[GetCol("StartDateTime")].ToString();
                Trace.WriteLine("Строка старого времени старта пуста = " + String.IsNullOrEmpty(oldStartDTStr));
                string oldEndDTStr = InDT.Rows[j].ItemArray[GetCol("EndDateTime")].ToString();
                Trace.WriteLine("Строка старого времени конца пуста = " + String.IsNullOrEmpty(oldEndDTStr));
                DateTime oldStartDT = new DateTime();
                oldStartDT = InDT.Rows[j].ItemArray[GetCol("StartDateTime")].ToString() == "" ? new DateTime() : Convert.ToDateTime(InDT.Rows[j].ItemArray[GetCol("StartDateTime")].ToString());
                Trace.WriteLine("oldStartDT > endDT = " + (oldStartDT > endDT));
                DateTime oldEndDT = new DateTime();
                oldEndDT = InDT.Rows[j].ItemArray[GetCol("EndDateTime")].ToString() == "" ? new DateTime() : Convert.ToDateTime(InDT.Rows[j].ItemArray[GetCol("EndDateTime")].ToString());
                Trace.WriteLine("oldEndDT < startDT = " + (oldEndDT < startDT));
                if (
                        idEquipment == Convert.ToInt32(InDT.Rows[j].ItemArray[GetCol("IdEquipment")].ToString())//строка с интересующим нас оборудованием
                        &&
                        (
                        //ToDO Ошибка здесь

                            (
                            //Время в строке не пустое
                            String.IsNullOrEmpty(oldStartDTStr) ||
                            String.IsNullOrEmpty(oldEndDTStr)
                            )
                            &&
                            !(
                                //Отрицание проверки на свободность
                                oldStartDT > endDT &&
                                oldEndDT < startDT
                            )
                        )
                    )
                     /*
                      * Если строка по интересующему нас оборудованию
                      * и
                      *     Время в строке не пустое
                      *     и
                      *     отрицание(время оборудования пустое)
                      */
                {
                    return false;
                }
            }
            return true;
        }
        
        bool IsFree2(int idEquipment, DateTime startDT, DateTime endDT)
		{
            for (int j = 0; j < InDT.Rows.Count; j++)
            {
                //Перечисление переменных
                //Trace.WriteLine("idEquipment = " + idEquipment);
                //Trace.WriteLine("startDT = " + startDT);
                //Trace.WriteLine("endDT = " + endDT);

                //Ищем строки с подходящим оборудованием
                //Trace.WriteLine("Оборудование совпадает = " + (idEquipment == Convert.ToInt32(OutDT.Rows[j].ItemArray[GetCol("IdEquipment")].ToString())));
                if (idEquipment == Convert.ToInt32(OutDT.Rows[j].ItemArray[GetCol("IdEquipment")].ToString()))
                {
                    //есть ли записи о времение в таблице
                    string oldStartDTStr = OutDT.Rows[j].ItemArray[GetCol("StartDateTime")].ToString();
                    string oldEndDTStr = OutDT.Rows[j].ItemArray[GetCol("EndDateTime")].ToString();
                    //Trace.WriteLine("Строка старого времени старта пуста = " + String.IsNullOrEmpty(oldStartDTStr));
                    //Trace.WriteLine("Строка старого времени конца пуста = " + String.IsNullOrEmpty(oldEndDTStr));
                    if (!(String.IsNullOrEmpty(oldStartDTStr) || String.IsNullOrEmpty(oldEndDTStr)))
					{
                        //Если пересека пересекаются
                        DateTime oldStartDT = new DateTime();
                        DateTime oldEndDT = new DateTime();
                        oldStartDT = (oldStartDTStr == "") ? new DateTime() : Convert.ToDateTime(oldStartDTStr);
                        oldEndDT = (oldEndDTStr == "") ? new DateTime() : Convert.ToDateTime(oldEndDTStr);
                        //Trace.WriteLine("oldStartDT > endDT = " + (oldStartDT > endDT));
                        //Trace.WriteLine("oldEndDT < startDT = " + (oldEndDT < startDT));
                        //Нужна проверка на исключение минимальной даты
                        if (oldStartDT > endDT && oldEndDT < startDT)
                            return false;
                    }
                }
            }
            return true;
        }

        public DataTable MainFun(Dictionary<string, decimal> variableDictionary, int trevelTime, DateTime maxDateTime, out string exStr)
        {
            ////Trace.WriteLine("Запустились");
            
            exStr = String.Empty;

			try
			{
				//Создание выходной таблицы
				OutDT = InDT;

                DateTime lastDT = new DateTime();

                for (int i = 0; i < OutDT.Rows.Count; i++)
				{
                    //Заменяем все формулы расчёта времени операции на их значения
                    int plastCount = Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("LastCount")]);
                    if (!variableDictionary.ContainsKey("plast"))
                    {
                        variableDictionary.Add("plast", Convert.ToDecimal(plastCount));
                    }
                    else
                    {
                        variableDictionary["plast"] = Convert.ToDecimal(plastCount);
                    }
                    OutDT.Rows[i].SetField(GetCol("TimeFormula"), Second.MainFun(OutDT.Rows[i].ItemArray[GetCol("TimeFormula")].ToString(), variableDictionary).ToString());
                }

                Trace.WriteLine("Начинаем цикл");
                for (int i = 0; i < OutDT.Rows.Count; i++)
                {
                    //Для последующего удобства выведем в переменную с 
                    int idBatch = Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("IdBatch")]);

                    /*
					//если это первая строка или строка для новой партии нужно обнулить счётчик до последнего времени.
					//если его нет то до первого времени.
					//Если и его нет то в идеале спросить у пользователя когда будет начинаться партия, но нам для начала пойдёт и настоящие время
					if (i == 0 || (i == 0 ? (OutDT.Rows[i].ItemArray[GetCol("IdBatch")] == OutDT.Rows[i - 1].ItemArray[GetCol("IdBatch")]) : true))
					{
						if (!String.IsNullOrEmpty(OutDT.Rows[i].ItemArray[GetCol("EndDateTime")].ToString()))
						{
							lastDT = Convert.ToDateTime(OutDT.Rows[i].ItemArray[GetCol("EndDateTime")].ToString());
						}
						else if (!String.IsNullOrEmpty(OutDT.Rows[i].ItemArray[GetCol("StartDateTime")].ToString()))
						{
							lastDT = Convert.ToDateTime(OutDT.Rows[i].ItemArray[GetCol("StartDateTime")].ToString());
						}
						else
						{
							lastDT = DateTime.Now;
						}
					}
                    */

                    /*
                     * Здесь делаем каскад if-ов управляющих lastDT
                     * если предыдущая строка той же партии - ничего не делаем с lastDT
                     * если строка из новой партии
                     *      пытаемся взять endDT
                     *      если endDT пустая - пытаемся взять из statDT
                     *      если и там пусто пишем сообщение в список ошибок и берём нынешнее время
                     */

                    //TODO Нужно переделать.
                    //Я тут взял ранее использованное решение и попытался его оживить

                    if(i > 0)
					{
                        if(idBatch == Convert.ToInt32(OutDT.Rows[i - 1].ItemArray[GetCol("IdBatch")]))
						{

						}
					}
                    else if (i == 0 || (i == 0 ? (OutDT.Rows[i].ItemArray[GetCol("IdBatch")] == OutDT.Rows[i - 1].ItemArray[GetCol("IdBatch")]) : true))
                    {
                        if (!String.IsNullOrEmpty(OutDT.Rows[i].ItemArray[GetCol("EndDateTime")].ToString()))
                        {
                            lastDT = Convert.ToDateTime(OutDT.Rows[i].ItemArray[GetCol("EndDateTime")].ToString());
                        }
                        else if (!String.IsNullOrEmpty(OutDT.Rows[i].ItemArray[GetCol("StartDateTime")].ToString()))
                        {
                            lastDT = Convert.ToDateTime(OutDT.Rows[i].ItemArray[GetCol("StartDateTime")].ToString());
                        }
                        else
                        {
                            lastDT = DateTime.Now;
                        }
                    }
                    

                    //Цикл на одну партию
                    //TODO нужно поменять.
                    //Возможно Нужно пролверять не со следующей партией. а с предыйдущей

                    Trace.WriteLine("--------------------------------------------------------------------------------------");
                    Trace.WriteLine("Строка " + i + " партии " + idBatch);
                    Trace.WriteLine("--------------------------------------------------------------------------------------");
                    Trace.WriteLine("I = " + i);
                    DateTime startDT = lastDT.AddMinutes(trevelTime);
                    DateTime endDT = startDT.AddMinutes(Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("TimeFormula")]));

                    //Проверяет свободен ли станок всё время. Выход из цикла происходит внутри цикла.
                    while (startDT <= maxDateTime)
                    {
                        if (IsFree2(Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("IdEquipment")]), startDT, endDT))
                        {
                            OutDT.Rows[i].SetField(GetCol("StartDateTime"), startDT);
                            OutDT.Rows[i].SetField(GetCol("EndDateTime"), endDT);
                            lastDT = endDT;
                            break;
                        }
                        else
                        {
                            startDT = startDT.AddMinutes(1);
                            endDT = endDT.AddMinutes(1);
                            lastDT = endDT;
                        }
                    }
                }

				return OutDT;
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
				exStr = ex.Message;
			}
			return OutDT;
		}

        public DataTable MainFun(DataTable newInDT, Dictionary<string, decimal> variableDictionary, Dictionary<string, int> newColDictionary, int trevelTime, DateTime maxDateTime, out string exStr)
        {
            InDT = newInDT;
            colDictionary = newColDictionary;
            return MainFun(variableDictionary, trevelTime, maxDateTime, out exStr);
        }
    }
}
