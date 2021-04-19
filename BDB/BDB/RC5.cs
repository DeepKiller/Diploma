using System;
using System.Text;

namespace BDB
{
    /// <summary>
    /// Класс для шифрования данных
    /// </summary>
    static class RC5
    {
        /// <summary>
        /// Размер половины блока
        /// </summary>
        private const byte HalfBlockSize_W = 64;
        /// <summary>
        /// Количество итераций
        /// </summary>
        private const byte Rounds_R = 255;
        /// <summary>
        /// Константа золотого сечения
        /// </summary>
        private const double GoldenRatio_F = 1.6180339887;
        /// <summary>
        /// Псевдо случайная величина
        /// </summary>
        static private readonly ulong P = Convert.ToUInt64(OddRound((GoldenRatio_F - 1) * Math.Pow(2, HalfBlockSize_W)));
        /// <summary>
        /// Псевдо случайная величина
        /// </summary>
        static private readonly ulong Q = Convert.ToUInt64(OddRound((Math.E - 2) * Math.Pow(2, HalfBlockSize_W)));
        /// <summary>
        /// Возвращает ближайшее нечётное число
        /// </summary>
        /// <param name="num">Число для округления</param>
        /// <returns>Возвращает округлённое число</returns>
        static private double OddRound(double num)
        {
            return Math.Round(num) % 2 == 0 ? Math.Round(num) - 1 : Math.Round(num);
        }
        /// <summary>
        /// Циклический сдвиг влево
        /// </summary>
        /// <param name="arg">Число для сдвига</param>
        /// <param name="n">Размер сдвига</param>
        /// <returns>arg сдвинутое циклически</returns>
        static private ulong ShiftCycleLeft(ulong arg, int n)
        {
            ulong r1, r2;
            r1 = arg << n;
            r2 = arg >> (HalfBlockSize_W - n);
            return (r1 | r2);
        }
        /// <summary>
        /// Циклический сдвиг вправо
        /// </summary>
        /// <param name="arg">Число для сдвига</param>
        /// <param name="n">Размер сдвига</param>
        /// <returns>arg сдвинутое циклически</returns>
        static private ulong ShiftCycleRigth(ulong arg, int n)
        {
            ulong r1, r2;
            r1 = arg >> n;
            r2 = arg << (HalfBlockSize_W - n);
            return (r1 | r2);
        }
        /// <summary>
        /// Выполняет шифрование данных
        /// </summary>
        /// <param name="Password">Пароль для шифрования</param>
        /// <param name="A">Первый блок данных</param>
        /// <param name="B">Второй блок данных</param>
        static public void CryptData(string Password, ref ulong A, ref ulong B)
        {
            ulong[] AdvancedKeys_S = new ulong[2 * (Rounds_R + 1)];
            RC5Prepare(ref Password, ref AdvancedKeys_S);
            A +=  AdvancedKeys_S[0];
            B +=  AdvancedKeys_S[1];
            for (int i = 1; i < Rounds_R+1; i++)
            {
                A = ShiftCycleLeft(A ^ B, (int)B) + AdvancedKeys_S[2 * i];
                B = ShiftCycleLeft(B ^ A, (int)A) + AdvancedKeys_S[2 * i + 1];
            }
        }
        /// <summary>
        /// Выполняет дешифрование данных
        /// </summary>
        /// <param name="Password">Пароль для дешифрования</param>
        /// <param name="A">Первый блок данных</param>
        /// <param name="B">Второй блок данных</param>
        static public void DeCryptData(string Password, ref ulong A, ref ulong B)
        {
            ulong[] AdvancedKeys_S = new ulong[2 * (Rounds_R + 1)];
            RC5Prepare(ref Password, ref AdvancedKeys_S);
            for (int i = Rounds_R; i >0; i--)
            {
                B = ShiftCycleRigth(B - AdvancedKeys_S[2 * i + 1], (int)A) ^ A;
                A = ShiftCycleRigth(A - AdvancedKeys_S[2 * i], (int)B) ^ B;
            }
            B -= AdvancedKeys_S[1];
            A -= AdvancedKeys_S[0];
        }
        /// <summary>
        /// Выполняет необходимые подготовки
        /// </summary>
        /// <param name="Password">Пароль</param>
        /// <param name="AdvancedKeys_S">Массив расшириных ключей</param>
        static private void RC5Prepare(ref string Password, ref ulong[] AdvancedKeys_S)
        {
            byte PasswordByteSize_B = Convert.ToByte(Encoding.UTF8.GetBytes(Password).Length);
            //разбиение ключа на слова
            int U = HalfBlockSize_W >> 3;
            int T = 2 * (Rounds_R + 1);
            int WordSize_C = PasswordByteSize_B % (HalfBlockSize_W >> 3) > 0 ? PasswordByteSize_B / U +1 : PasswordByteSize_B / U;
            ulong[] WordArray_L = new ulong[WordSize_C];
            for (int i = PasswordByteSize_B - 1; i >= 0; i--)
                WordArray_L[i / U] = ShiftCycleRigth(WordArray_L[i / U], 8) + Encoding.UTF8.GetBytes(Password)[i];
            //Построение таблицы расшириных ключей
            AdvancedKeys_S[0] = P;
            for (int i = 1; i < AdvancedKeys_S.Length; i++)
                AdvancedKeys_S[i] = AdvancedKeys_S[i - 1] + Q;
            //Перемешивание
            long IterationCount_N = 3 * Math.Max(WordSize_C, T);
            ulong G, H;
            int k, l;
            G = H = 0;
            k = l = 0;
            while (IterationCount_N > 0)
            {
                G = AdvancedKeys_S[k] = ShiftCycleLeft((AdvancedKeys_S[k] + G + H), 3);
                H = WordArray_L[l] = ShiftCycleLeft((WordArray_L[l] + G + H), (int)(G + H));
                k = (k + 1) % T;
                l = (l + 1) % WordSize_C;
                IterationCount_N--;
            }
        }
    }
}
