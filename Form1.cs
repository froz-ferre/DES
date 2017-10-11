using System;
using System.Windows.Forms;
using System.IO;

namespace LR5
{
    public partial class Form1 : Form
    {
        private const int sizeOfBlock = 64 * 2; // в DES размер блока 64 бит, но поскольку в unicode символ в два раза длинее, то увеличим блок тоже в два раза
        private const int sizeOfChar = 16; // размер одного символа (in Unicode 16 bit)

        private const int shiftKey = 2; // сдвиг ключа 

        private const int quantityOfRounds = 16; // количество раундов

        string[] Blocks; // сами блоки в двоичном формате

        public Form1()
        {
            InitializeComponent();
        }

        private string StringToRightLength(string input)
        {
            while (((input.Length * sizeOfChar) % sizeOfBlock) != 0) // делаем исходный текст кратным размеру блока 
                input += " "; // если не кратен, то дополняем пробелы

            return input;
        }

        // перевод сроки из символьного в двоичный формат
        private string StringToBinaryFormat(string input)
        {
            string output = "";

            for (int i = 0; i < input.Length; i++)
            {
                string char_binary = Convert.ToString(input[i], 2); // переводим символ в двоичный формат

                while (char_binary.Length < sizeOfChar) // если длина текущего двоичного символа меньше размера символа в формате Unicode
                    char_binary = "0" + char_binary; // то добавляем в начало нули

                output += char_binary;
            }

            return output;
        }

        // разбиваем исходный текст на DES-блоки 
        private void CutStringIntoBlocks(string input)
        {
            Blocks = new string[(input.Length * sizeOfChar) / sizeOfBlock]; // создаем массив размером в нужное кол-во блоков

            int lengthOfBlock = input.Length / Blocks.Length; // длинна одного блока

            for (int i = 0; i < Blocks.Length; i++) // записываем в каждый блок данные с исходного текста
            {
                Blocks[i] = input.Substring(i * lengthOfBlock, lengthOfBlock); // непосредственно разбиваем исходный текст на подстроки, то бишь DES-блоки - символьно
                Blocks[i] = StringToBinaryFormat(Blocks[i]); // переводим данные блоков из символьного формата в битовый
            }
        }

        // разбиваем на блоки в двоичном формате
        private void CutBinaryStringIntoBlocks(string input)
        {
            Blocks = new string[input.Length / sizeOfBlock];

            int lengthOfBlock = input.Length / Blocks.Length;

            for (int i = 0; i < Blocks.Length; i++)
                Blocks[i] = input.Substring(i * lengthOfBlock, lengthOfBlock);
        }

        // приводим ключ к нужной длине
        private string CorrectKeyWord(string input, int lengthKey)
        {
            if (input.Length > lengthKey) // если длина ключа больше требуемой длине ключа
                input = input.Substring(0, lengthKey); // то обрезаем ключ до нужной длинны
            else
                while (input.Length < lengthKey) // если длина ключа меньше требуемой длине ключа
                    input = "0" + input; // то добавляе в начало нули

            return input; 
        }

        // шифрование DES - один раунд
        private string EncodeDES_One_Round(string input, string key)
        {
            // делим строку на две равные части
            string L = input.Substring(0, input.Length / 2); // левая часть
            string R = input.Substring(input.Length / 2, input.Length / 2); // правая часть

            StreamWriter writer = new StreamWriter("process_crypt.txt", true, System.Text.Encoding.Default);
            writer.WriteLine("R = " + R + ", L = " + L);
            writer.Close();

            return (R + XOR(L, XOR(R, key)));
        }

        // дешифрование DES - один раунд
        private string DecodeDES_One_Round(string input, string key)
        {
            // делим строку на две равные части
            string L = input.Substring(0, input.Length / 2); // левая часть
            string R = input.Substring(input.Length / 2, input.Length / 2); // правая часть

            return (XOR(XOR(L, key), R) + L);
        }

        /* Исключающее« ИЛИ» XOR (или сложение по модулю 2) — это бинарная операция, 
         * результат действия которой равен 1, если число складываемых единичных битов нечётно 
         * и равен 0, если чётно. 
         * Другими словами, если оба соответствующих бита операндов равны между собой, двоичный разряд результата равен 0; 
         * в противном случае, двоичный разряд результата равен 1. */

        // XOR двух строк с двоичными данными
        private string XOR(string s1, string s2)
        {
            string result = "";

            for (int i = 0; i < s1.Length; i++)
            {
                bool a = Convert.ToBoolean(Convert.ToInt32(s1[i].ToString()));
                bool b = Convert.ToBoolean(Convert.ToInt32(s2[i].ToString()));

                if (a ^ b)
                    result += "1";
                else
                    result += "0";
            }
            return result;
        }

        // Вычисление ключа для следующего раунда шифрования DES. Циклический сдвиг вправо на 2.
        private string KeyToNextRound(string key)
        {
            for (int i = 0; i < shiftKey; i++)
            {
                // перемещаем последний символ на первое место
                key = key[key.Length - 1] + key;
                key = key.Remove(key.Length - 1);
            }

            return key;
        }

        // Вычисление ключа для следующего раунда шифрования DES. Циклический сдвиг влево на 2.
        private string KeyToPrevRound(string key)
        {
            for (int i = 0; i < shiftKey; i++)
            {
                // перемещаем первы символ на последнее место
                key = key + key[0];
                key = key.Remove(0, 1);
            }

            return key;
        }

        // перевод строки из двоичного в символьный формат
        private string StringFromBinaryToNormalFormat(string input)
        {
            string output = "";

            while (input.Length > 0)
            {
                string char_binary = input.Substring(0, sizeOfChar);
                input = input.Remove(0, sizeOfChar);

                int a = 0;
                int degree = char_binary.Length - 1;

                foreach (char c in char_binary)
                    a += Convert.ToInt32(c.ToString()) * (int)Math.Pow(2, degree--);

                output += ((char)a).ToString();
            }

            return output;
        }

        private void buttonCrypt_Click(object sender, EventArgs e)
        {
            if (keyBox.Text.Length > 0)
            {
                string s = sourseBox.Text;
                string key = keyBox.Text;

                s = StringToRightLength(s);
                CutStringIntoBlocks(s);

                key = CorrectKeyWord(key, s.Length / (2 * Blocks.Length));
                keyBox.Text = key;
                key = StringToBinaryFormat(key);

                for (int j = 0; j < quantityOfRounds; j++)
                {
                    for (int i = 0; i < Blocks.Length; i++)
                        Blocks[i] = EncodeDES_One_Round(Blocks[i], key);

                    key = KeyToNextRound(key);
                }

                key = KeyToPrevRound(key);

                keyBox.Text = StringFromBinaryToNormalFormat(key);

                string result = "";

                for (int i = 0; i < Blocks.Length; i++)
                    result += Blocks[i];

                cryptBox.Text = StringFromBinaryToNormalFormat(result);
            }
            else
                MessageBox.Show("Введите ключевое слово!");
        }

        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            if (keyBox.Text.Length > 0)
            {
                string s = cryptBox.Text;
                string key = StringToBinaryFormat(keyBox.Text);

                s = StringToBinaryFormat(s);
                CutBinaryStringIntoBlocks(s);

                for (int j = 0; j < quantityOfRounds; j++)
                {
                    for (int i = 0; i < Blocks.Length; i++)
                        Blocks[i] = DecodeDES_One_Round(Blocks[i], key);

                    key = KeyToPrevRound(key);
                }

                key = KeyToNextRound(key);

                keyBox.Text = StringFromBinaryToNormalFormat(key);

                string result = "";

                for (int i = 0; i < Blocks.Length; i++)
                    result += Blocks[i];

                sourseBox.Text = StringFromBinaryToNormalFormat(result);
            }
            else
                MessageBox.Show("Введите ключевое слово!");
        }

        private void openSourseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control ctrl = this.ActiveControl;
            if (ctrl is TextBox)
            {
                TextBox tx = (TextBox)ctrl;
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
                string filename = openFileDialog1.FileName;
                string fileText = File.ReadAllText(filename);
                tx.Text = fileText;
            }
                    
        }

        private void saveSourseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Control ctrl = this.ActiveControl;
            if (ctrl is TextBox)
            {
                TextBox tx = (TextBox)ctrl;
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel) return;
                string filename = saveFileDialog1.FileName;
                File.WriteAllText(filename, tx.Text);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
