using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Diagnostics;

namespace DistributionLib
{
    public class Discret1
    {
        static DataTable inDT = new DataTable("In data table");
        /*
SELECT
Batchs.Priority as 'Приоритет партии',
Batchs.IdBatch as 'ID партии',
--Batchs.Name as 'Название партии',
Operations.Number as 'Номер операции',
--Operations.IdOperation as 'ID операции',
--Operations.IdMSL as 'ID МСЛ',
--Operations.Name as 'Название операций',
Equipments.IdEquipment as 'ID оборудование',
--Equipments.Name as 'Название оборудования',
Devices.IdDevice as 'ID девайса',
--Devices.KeyDevice as 'Код девайса',
Devices.Name as 'Название девайса'
FROM Operations

LEFT JOIN MSLs ON Operations.IdMSL = MSLs.IdMSL
LEFT JOIN Devices ON MSLs.IdDevice = Devices.IdDevice
--Возможно нужно RIGHT JOIN V
LEFT JOIN Batchs ON MSLs.IdMSL = Batchs.IdMSL

LEFT JOIN Routing ON Operations.IdRouting = Routing.IdRouting
--Наверняка можно упростить V
LEFT JOIN TechnologicalMaps ON Routing.IdTM = TechnologicalMaps.IdTM
LEFT JOIN EquipmentsTM ON TechnologicalMaps.IdTM = EquipmentsTM.IdTM
LEFT JOIN EquipmentsCertificates ON EquipmentsTM.IdEquipmentCertificate = EquipmentsCertificates.IdCertificate
LEFT JOIN Equipments ON EquipmentsCertificates.IdEquipment = Equipments.IdEquipment

ORDER BY Batchs.Priority DESC, Batchs.IdBatch ASC, Operations.Number ASC
         */

        static DataTable outDT = new DataTable("Out data table");

        static DataTable equipments = new DataTable("equipments data table");
        /*
SELECT IdEquipment, Name FROM Equipments
ORDER BY IdEquipment ASC
        */

        public static void FormingODT(DataTable newEquipmentsDT)
        {
            outDT.Columns.Add("EquipID", typeof(int));
            outDT.Columns.Add("EquipName", typeof(string));

            //Делаем столбцы для времени
            for (int a = 0; a < (4 * 12); a++)
            {
                outDT.Columns.Add(Convert.ToString(a + 1), typeof(int));
            }

            //Делаем колличество строк равные колличеству станков
            equipments = newEquipmentsDT;

            while (equipments.Rows.Count > outDT.Rows.Count)
            {
                outDT.Rows.Add();
                outDT.Rows[outDT.Rows.Count - 1].SetField(0, equipments.Rows[outDT.Rows.Count - 1].ItemArray[0]);
                outDT.Rows[outDT.Rows.Count - 1].SetField(1, equipments.Rows[outDT.Rows.Count - 1].ItemArray[1]);
            }
        }

        public static void Сalculation()
        {
            int lastTime = 0 + 2;//погрешность связана с двумя дополнительными столбцами в выходной таблице с данными об оборудовании

            for (int k = 0; k < inDT.Rows.Count; k++)
            {
                int i = Convert.ToInt32(inDT.Rows[k].ItemArray[3]) - 1; // Номер строки в выходной строке равен id  обурудования полученного
                //TODO Надо править ибо нет проверки что id оборудования равен строке в выходной таблице с соответствующем оборудованием.
                //По хорошему надо бежать по строкам выходной таблице и сверять id оборудования там и принимать номер той строки за i
                Trace.WriteLine("i: " + i);

                Trace.WriteLine("lastTime = " + lastTime);

                for (int j = lastTime; j < outDT.Columns.Count; j++)
                {
                    Trace.WriteLine("\tj: " + j);

                    Trace.WriteLine("\t\tПусто ли: " + (outDT.Rows[i].ItemArray[j] == DBNull.Value));
                    if (outDT.Rows[i].ItemArray[j] == DBNull.Value) //Проверка на пустоту
                    {
                        outDT.Rows[i].SetField(j, inDT.Rows[k].ItemArray[1]);
                        Trace.WriteLine("\t\t\t" + outDT.Rows[i].ItemArray[j]);
                        lastTime = j + 1;
                        Trace.WriteLine("\t\t\tlastTime = " + lastTime);
                        break;
                    }
                }
                Trace.WriteLine("Есть ли следующая строка: " + (k + 1 < inDT.Rows.Count));
                if (k + 1 < inDT.Rows.Count) //Если есть следующая партия
                {
                    Trace.WriteLine("Та же ли там партия: " + (inDT.Rows[k + 1].ItemArray[1] == inDT.Rows[k].ItemArray[1]));
                    if (Convert.ToInt32(inDT.Rows[k + 1].ItemArray[1]) != Convert.ToInt32(inDT.Rows[k].ItemArray[1])) // Если следующая строка НЕ касается той же партии
                        lastTime = 2; // Обнуляем начальное время
                }
            }
        }

        public static DataTable DurationMainFun(DataTable newEquipmentsDT, DataTable newInDT)
        {
            inDT = newInDT;

            FormingODT(newEquipmentsDT);

            Сalculation();

            return outDT;
        }
    }
}
