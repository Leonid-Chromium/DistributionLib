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
                Trace.WriteLine("idEquipment == Convert.ToInt32(InDT.Rows[j].ItemArray[GetCol(\"IdEquipment\")].ToString()) = " + (idEquipment == Convert.ToInt32(InDT.Rows[j].ItemArray[GetCol("IdEquipment")].ToString())));
                Trace.WriteLine("String.IsNullOrEmpty(InDT.Rows[j].ItemArray[GetCol(\"StartDateTime\")].ToString()) = " + String.IsNullOrEmpty(InDT.Rows[j].ItemArray[GetCol("StartDateTime")].ToString()));
                Trace.WriteLine("String.IsNullOrEmpty(InDT.Rows[j].ItemArray[GetCol(\"EndDateTime\")].ToString()) = " + String.IsNullOrEmpty(InDT.Rows[j].ItemArray[GetCol("EndDateTime")].ToString()));
                Trace.WriteLine("Convert.ToDateTime(InDT.Rows[j].ItemArray[GetCol(\"StartDateTime]\")].ToString()) > endDT = " + (Convert.ToDateTime(InDT.Rows[j].ItemArray[GetCol("StartDateTime")].ToString()) > endDT));
                Trace.WriteLine("Convert.ToDateTime(InDT.Rows[j].ItemArray[GetCol(\"EndDateTime\")].ToString()) < startDT = " + (Convert.ToDateTime(InDT.Rows[j].ItemArray[GetCol("EndDateTime")].ToString()) < startDT));
                if (
                        idEquipment == Convert.ToInt32(InDT.Rows[j].ItemArray[GetCol("IdEquipment")].ToString())//строка с интересующим нас оборудованием
                        &&
                        (
                            (
                            //Время в строке не пустое
                            String.IsNullOrEmpty(InDT.Rows[j].ItemArray[GetCol("StartDateTime")].ToString()) ||
                            String.IsNullOrEmpty(InDT.Rows[j].ItemArray[GetCol("EndDateTime")].ToString())
                            )
                            &&
                            !(
                                //Отрицание проверки на свободность
                                Convert.ToDateTime(InDT.Rows[j].ItemArray[GetCol("StartDateTime")].ToString()) > endDT &&
                                Convert.ToDateTime(InDT.Rows[j].ItemArray[GetCol("EndDateTime")].ToString()) < startDT
                            )
                        )
                    )
                     /*
                      * Если строка по интересующему нас оборудованию
                      * и
                      *     Время в строке не пустое
                      *     и
                      *     отрицание(время пустое)
                      */
                {
                    return false;
                }
            }
            return true;
        }

        public DataTable MainFun(Dictionary<string, decimal> variableDictionary, int trevelTime, DateTime maxDateTime, out string exStr)
        {
            Trace.WriteLine("Запустились");
            DataTable OutDT = new DataTable("Output data table");
            exStr = String.Empty;

            try
            {
                //Создание выходной таблицы
                OutDT = InDT;

                DateTime lastDT = new DateTime();

                Trace.WriteLine("Начинаем цикл");
                for (int i = 0; i < OutDT.Rows.Count; i++)
                {
                    int plastCount = Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("LastCount")]);
                    variableDictionary.Add("plast", Convert.ToDecimal(plastCount));
                    Trace.WriteLine("ААААААА сука");
                    //Заменяем все формулы расчёта времени операции на их значения
                    OutDT.Rows[i].SetField(GetCol("TimeFormula"), Second.MainFun(OutDT.Rows[i].ItemArray[GetCol("TimeFormula")].ToString(), variableDictionary).ToString());
                    //TODO fgxngfhkjnbzvd
                    Trace.WriteLine("fgdfhjkfjthtewghjmfytdjhghjgyujhgdxctkmnxdfgdrhftyjrgbbhk,lhbfbh = " + i );

                    //если это первая строка или строка для новой партии нужно обнулить счётчик до последнего времени.
                    //если его нет то до первого времени.
                    //Если и его нет то в идеале спросить у пользователя когда будет начинаться партия, но нам для начала пойдёт и настоящие время
                    if (i == 0 || (i == 0 ? (OutDT.Rows[i].ItemArray[GetCol("IdBatch")] == OutDT.Rows[i-1].ItemArray[GetCol("IdBatch")]) : true))
					{
                        if(!String.IsNullOrEmpty(OutDT.Rows[i].ItemArray[GetCol("EndDateTime")].ToString()))
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

                    //Для последующего удобства выведем в переменную с 
                    int idBatch = Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("IdBatch")]);
                    bool a = i + 1 < OutDT.Rows.Count;
                    bool b = idBatch == Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("IdBatch") + 1]);

                    Trace.WriteLine("fdgzxhj" + i);

                    DateTime startDT = lastDT.AddMinutes(trevelTime);
                    DateTime endDT = startDT.AddMinutes(Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("TimeFormula")]));

                    while (startDT <= maxDateTime)
                    {
                        if (IsFree(Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("IdEquipment")]), startDT, endDT))
                        {
                            OutDT.Rows[i].SetField(GetCol("StartDateTime"), startDT);
                            OutDT.Rows[i].SetField(GetCol("EndDateTime"), endDT);
                            lastDT = endDT;
                        }
                        else
                        {
                            startDT = startDT.AddMinutes(1);
                            endDT = endDT.AddMinutes(1);
                            lastDT = endDT;
                        }
                    }
                    i++;

                    //               //Что это за цикл
                    //               while (i + 1 < OutDT.Rows.Count /*&& Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("IdBatch") + 1])*/)
                    //{
                    //                   Trace.WriteLine("fdgzxhj" + i);

                    //                   DateTime startDT = lastDT.AddMinutes(trevelTime);
                    //                   DateTime endDT = startDT.AddMinutes(Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("TimeFormula")]));

                    //                   while(startDT <= maxDateTime)
                    //	{
                    //                       if (IsFree(Convert.ToInt32(OutDT.Rows[i].ItemArray[GetCol("IdEquipment")]), startDT, endDT))
                    //		{
                    //                           OutDT.Rows[i].SetField(GetCol("StartDateTime"), startDT);
                    //                           OutDT.Rows[i].SetField(GetCol("EndDateTime"), endDT);
                    //                           lastDT = endDT;
                    //                       }
                    //                       else
                    //		{
                    //                           startDT = startDT.AddMinutes(1);
                    //                           endDT = endDT.AddMinutes(1);
                    //                           lastDT = endDT;
                    //		}
                    //	}
                    //                   i++;
                    //}

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
