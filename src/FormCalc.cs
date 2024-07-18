// Calculator
// Copyright (C) 2024 koltsol33@mail.ru

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace _calculator
{
    public partial class FormCalc : Form
    {
        public class Node
        {
            public decimal element;
            public Node next;

            public Node() { }

            public Node(decimal element)
            {
                this.element = element;
            }
        }

        public class Stack
        {
            public Node top;

            public Stack()
            {
                top = null;
            }

            public void Push(decimal el)
            {
                Node newTop = new Node(el);

                if (top != null)
                {
                    newTop.next = top;
                    top = newTop;
                }

                else
                {
                    top = newTop;
                }
            }

            public decimal Pop()
            {
                decimal el;

                if (top != null)
                {
                    el = top.element;
                    top = top.next;
                }

                else
                {
                    el = -13;
                }

                return el;
            }
        }

        const int LEFT = -1;
        const int RIGHT = -2;
        const int ADD = -3;
        const int SUB = -4;
        const int MUL = -5;
        const int DIV = -6;
        const int ERR = -13;

        const string ERROR_MESSAGE = "Error";

        bool isDegreeMode = false;
        bool isEngineeringMode = false;

        Stack stack = new Stack();

        static decimal[] ConvertExpressionToTokens(string expression)
        {
            decimal[] arrayOfTokens = new decimal[1000];
            int indexOfCurrentToken = 0;
            char currentChar;
            bool isPreviousCharDigit = false;
            bool isWritingDecimalPlacesMode = false;
            int countDecimalPlaces = 0;

            for (int indexOfCurrentChar = 0; indexOfCurrentChar < expression.Length; indexOfCurrentChar++)
            {
                currentChar = expression[indexOfCurrentChar];

                switch (currentChar)
                {
                    case '(':
                        if (isPreviousCharDigit)
                        {
                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken] = LEFT;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case ')':
                        if (isPreviousCharDigit)
                        {
                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken] = RIGHT;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case '+':
                        if (isPreviousCharDigit)
                        {
                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken] = ADD;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case '-':
                        if (isPreviousCharDigit)
                        {
                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken] = SUB;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case '*':
                        if (isPreviousCharDigit)
                        {
                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken] = MUL;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case '/':
                        if (isPreviousCharDigit)
                        {
                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken] = DIV;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    default:
                        if (char.IsDigit(currentChar))
                        {
                            if (isWritingDecimalPlacesMode)
                            {
                                countDecimalPlaces++;
                            }

                            if (arrayOfTokens[indexOfCurrentToken] == 0)
                            {
                                arrayOfTokens[indexOfCurrentToken] = int.Parse(Convert.ToString(currentChar));
                                isPreviousCharDigit = true;
                            }

                            else
                            {
                                arrayOfTokens[indexOfCurrentToken] = arrayOfTokens[indexOfCurrentToken] * 10 
                                    + int.Parse(Convert.ToString(currentChar));
                            }
                        }

                        if (currentChar == ',')
                        {
                            isWritingDecimalPlacesMode = true;
                        }

                        if ((indexOfCurrentChar == expression.Length - 1 || !char.IsDigit(expression[indexOfCurrentChar + 1])) 
                            && isWritingDecimalPlacesMode)
                        {
                            arrayOfTokens[indexOfCurrentToken] /= (decimal)Math.Pow(10, countDecimalPlaces);
                            countDecimalPlaces = 0;
                            isWritingDecimalPlacesMode = false;
                        }

                        break;
                }
            }

            if (isPreviousCharDigit)
            {
                indexOfCurrentToken += 1;
            }

            Array.Resize(ref arrayOfTokens, indexOfCurrentToken);

            return arrayOfTokens;
        }

        public bool CheckCorrectness(decimal[] arrayOfTokens)
        {
            int currentIndex = 0;
            bool isCorrect = true;

            while (currentIndex < arrayOfTokens.Length && isCorrect)
            {
                if (arrayOfTokens[currentIndex] == LEFT)
                {
                    stack.Push(LEFT);
                }

                if (arrayOfTokens[currentIndex] == RIGHT)
                {
                    if (stack.top != null && stack.top.element == LEFT)
                    {
                        stack.Pop();
                    }

                    else
                    {
                        isCorrect = false;
                    }
                }

                if (currentIndex == 0 && !(arrayOfTokens[currentIndex] >= -1))
                {
                    isCorrect = false;
                    break;
                }

                switch (arrayOfTokens[currentIndex])
                {
                    case LEFT:
                        if (!(currentIndex < arrayOfTokens.Length - 2 && ((arrayOfTokens[currentIndex + 1] >= 0 
                            && arrayOfTokens[currentIndex + 2] <= ADD) | arrayOfTokens[currentIndex + 1] == LEFT)))
                        {
                            isCorrect = false;
                        }
                        break;
                    case RIGHT:
                        if (!((currentIndex < arrayOfTokens.Length - 1 && arrayOfTokens[currentIndex + 1] <= RIGHT) 
                            | currentIndex == arrayOfTokens.Length - 1))
                        {
                            isCorrect = false;
                        }
                        break;
                    default:
                        if (arrayOfTokens[currentIndex] <= -3)
                        {
                            if (!(currentIndex < arrayOfTokens.Length - 1 && arrayOfTokens[currentIndex + 1] >= LEFT))
                            {
                                isCorrect = false;
                            }
                        }

                        if (arrayOfTokens[currentIndex] >= 0)
                        {
                            if (currentIndex < arrayOfTokens.Length - 1 && arrayOfTokens[currentIndex + 1] == LEFT)
                            {
                                isCorrect = false;
                            }
                        }
                        break;
                }

                currentIndex++;
            }

            if (stack.top != null)
            {
                isCorrect = false;
            }

            if (!isCorrect)
            {
                lblExpression.Text = ERROR_MESSAGE;
            }

            return isCorrect;
        }

        public decimal[] ConvertToReversePolishNotation(decimal[] arrayOfTokens)
        {
            int indexOfCurrentTokenInArray = 0;
            int currentIndexInPolishNotation = 0;

            while (indexOfCurrentTokenInArray < arrayOfTokens.Length)
            {
                if (arrayOfTokens[indexOfCurrentTokenInArray] >= 0)
                {
                    arrayOfTokens[currentIndexInPolishNotation] = arrayOfTokens[indexOfCurrentTokenInArray];
                    currentIndexInPolishNotation++;
                }

                else
                {
                    if (arrayOfTokens[indexOfCurrentTokenInArray] == LEFT)
                    {
                        stack.Push(arrayOfTokens[indexOfCurrentTokenInArray]);
                    }

                    else
                    {
                        if (arrayOfTokens[indexOfCurrentTokenInArray] == RIGHT)
                        {
                            while (stack.top.element != LEFT)
                            {
                                arrayOfTokens[currentIndexInPolishNotation] = Convert.ToInt32(stack.Pop());
                                currentIndexInPolishNotation++;
                            }

                            stack.Pop();
                        }

                        else
                        {
                            while (stack.top != null && stack.top.element <= arrayOfTokens[indexOfCurrentTokenInArray])
                            {
                                arrayOfTokens[currentIndexInPolishNotation] = Convert.ToInt32(stack.Pop());
                                currentIndexInPolishNotation++;
                            }

                            stack.Push(arrayOfTokens[indexOfCurrentTokenInArray]);
                        }
                    }
                }

                indexOfCurrentTokenInArray++;
            }

            while (stack.top != null)
            {
                arrayOfTokens[currentIndexInPolishNotation] = Convert.ToInt32(stack.Pop());
                currentIndexInPolishNotation++;
            }

            Array.Resize(ref arrayOfTokens, currentIndexInPolishNotation);

            return arrayOfTokens;
        }

        public decimal CalculateExpression(decimal[] arrayOfTokens)
        {
            decimal PerformOperation(decimal arithmeticOperator, decimal a, decimal b)
            {
                try
                {
                    switch (arithmeticOperator)
                    {
                        case ADD: return a + b;
                        case SUB: return a - b;
                        case MUL: return a * b;
                        case DIV: return a / b;
                        default: return ERR;
                    }
                }

                catch
                {
                    return ERR;
                }
            }

            decimal result = ERR;
            int currentIndex = 0;
            decimal firstOperand;
            decimal secondOperand;

            while (currentIndex < arrayOfTokens.Length)
            {
                if (arrayOfTokens[currentIndex] >= 0)
                {
                    stack.Push(arrayOfTokens[currentIndex]);
                }

                else
                {
                    secondOperand = Convert.ToDecimal(stack.Pop());
                    firstOperand = Convert.ToDecimal(stack.Pop());

                    result = PerformOperation(arrayOfTokens[currentIndex], firstOperand, secondOperand);
                    stack.Push(result);
                }

                currentIndex++;
            }

            stack.Pop();

            return result;
        }

        public FormCalc()
        {
            InitializeComponent();
        }

        private void FormCalc_Load(object sender, EventArgs e)
        {
            btn0.Click += new EventHandler(input_Click);
            btn1.Click += new EventHandler(input_Click);
            btn2.Click += new EventHandler(input_Click);
            btn3.Click += new EventHandler(input_Click);
            btn4.Click += new EventHandler(input_Click);
            btn5.Click += new EventHandler(input_Click);
            btn6.Click += new EventHandler(input_Click);
            btn7.Click += new EventHandler(input_Click);
            btn8.Click += new EventHandler(input_Click);
            btn9.Click += new EventHandler(input_Click);
            btnAddition.Click += new EventHandler(input_Click);
            btnSubtraction.Click += new EventHandler(input_Click);
            btnMultiplication.Click += new EventHandler(input_Click);
            btnDivision.Click += new EventHandler(input_Click);
            btnLeftBracket.Click += new EventHandler(input_Click);
            btnRightBracket.Click += new EventHandler(input_Click);
            btnDecimalSeparator.Click += new EventHandler(input_Click);
            btnDegree.Click += new EventHandler(input_Click);

            btnSin.Visible = false;
            btnCos.Visible = false;
            btnLn.Visible = false;
            btnSquareRoot.Visible = false;
            btnDegree.Visible = false;

            this.Width = 212;
        }

        private void input_Click(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;

            if ((lblExpression.Text == "0" || lblExpression.Text == ERROR_MESSAGE) 
                && clickedButton.Text != "," && clickedButton.Text != "^")
            {
                lblExpression.Text = clickedButton.Text;
            }
            else
            {
                lblExpression.Text += clickedButton.Text;
            }

            btnEquals.Focus();
        }

        private void btnBackSpace_Click(object sender, EventArgs e)
        {
            if (lblExpression.Text.Length > 1 && lblExpression.Text != ERROR_MESSAGE)
            {
                lblExpression.Text = lblExpression.Text.Remove(lblExpression.Text.Length - 1);
            }
            else
            {
                lblExpression.Text = "0";
            }

            btnEquals.Focus();
        }

        private void btnEquals_Click(object sender, EventArgs e)
        {
            if (isDegreeMode)
            {
                try
                {
                    double[] operands = lblExpression.Text.Split('^').Select(double.Parse).ToArray();

                    if (operands.Length == 2)
                    {
                        double result = Math.Pow(operands[0], operands[1]);
                        lblExpression.Text = result.ToString();
                    }
                    else
                    {
                        lblExpression.Text = ERROR_MESSAGE;
                    }
                }

                catch
                {
                    lblExpression.Text = ERROR_MESSAGE;
                }

            }
            else
            {
                decimal[] arrayOfTokens = ConvertExpressionToTokens(lblExpression.Text);

                if (CheckCorrectness(arrayOfTokens))
                {
                    arrayOfTokens = ConvertToReversePolishNotation(arrayOfTokens);

                    decimal result = CalculateExpression(arrayOfTokens);

                    if (result != ERR)
                    {
                        lblExpression.Text = result.ToString();
                    }
                    else
                    {
                        lblExpression.Text = ERROR_MESSAGE;
                    }
                }
            }
        }

        private double ConvertDegreesToRadians(double angleInDegrees)
        {
            return angleInDegrees * Math.PI / 180;
        }

        private void btnSin_Click(object sender, EventArgs e)
        {
            try
            {
                double result = Math.Sin(ConvertDegreesToRadians(Convert.ToDouble(lblExpression.Text)));
                lblExpression.Text = result.ToString();
            }

            catch
            {
                lblExpression.Text = ERROR_MESSAGE;
            }

            finally
            {
                btnEquals.Focus();
            }
        }

        private void btnCos_Click(object sender, EventArgs e)
        {
            try
            {
                double result = Math.Cos(ConvertDegreesToRadians(Convert.ToDouble(lblExpression.Text)));
                lblExpression.Text = result.ToString();
            }

            catch
            {
                lblExpression.Text = ERROR_MESSAGE;
            }

            finally
            {
                btnEquals.Focus();
            }
        }

        private void btnLn_Click(object sender, EventArgs e)
        {
            try
            {
                double result = Math.Log(Convert.ToDouble(lblExpression.Text));
                lblExpression.Text = result.ToString();
            }

            catch
            {
                lblExpression.Text = ERROR_MESSAGE;
            }

            finally
            {
                btnEquals.Focus();
            }
        }

        private void btnSquareRoot_Click(object sender, EventArgs e)
        {
            try
            {
                double result = Math.Sqrt(Convert.ToDouble(lblExpression.Text));
                lblExpression.Text = result.ToString();
            }

            catch
            {
                lblExpression.Text = ERROR_MESSAGE;
            }

            finally
            {
                btnEquals.Focus();
            }
        }

        private void btnDegree_Click(object sender, EventArgs e)
        {
            isDegreeMode = true;

            btnEquals.Focus();
        }

        private void FormCalc_KeyDown(object sender, KeyEventArgs e)
        {
            Button clickedButton = null;

            switch (e.KeyCode)
            {
                case Keys.D0:
                    if (e.Shift)
                    {
                        clickedButton = btnRightBracket;
                    }
                    else
                    {
                        clickedButton = btn0;
                    }
                    break;
                case Keys.NumPad0:
                    clickedButton = btn0;
                    break;
                case Keys.D1:
                    clickedButton = btn1;
                    break;
                case Keys.NumPad1:
                    clickedButton = btn1;
                    break;
                case Keys.D2:
                    clickedButton = btn2;
                    break;
                case Keys.NumPad2:
                    clickedButton = btn2;
                    break;
                case Keys.D3:
                    clickedButton = btn3;
                    break;
                case Keys.NumPad3:
                    clickedButton = btn3;
                    break;
                case Keys.D4:
                    clickedButton = btn4;
                    break;
                case Keys.NumPad4:
                    clickedButton = btn4;
                    break;
                case Keys.D5:
                    clickedButton = btn5;
                    break;
                case Keys.NumPad5:
                    clickedButton = btn5;
                    break;
                case Keys.D6:
                    if (e.Shift)
                    {
                        clickedButton = btnDegree;
                    }
                    else
                    {
                        clickedButton = btn6;
                    }
                    break;
                case Keys.NumPad6:
                    clickedButton = btn6;
                    break;
                case Keys.D7:
                    clickedButton = btn7;
                    break;
                case Keys.NumPad7:
                    clickedButton = btn7;
                    break;
                case Keys.D8:
                    if (e.Shift)
                    {
                        clickedButton = btnMultiplication;
                    }
                    else
                    {
                        clickedButton = btn8;
                    }
                    break;
                case Keys.NumPad8:
                    clickedButton = btn8;
                    break;
                case Keys.Multiply:
                    clickedButton = btnMultiplication;
                    break;
                case Keys.D9:
                    if (e.Shift)
                    {
                        clickedButton = btnLeftBracket;
                    }
                    else
                    {
                        clickedButton = btn9;
                    }
                    break;
                case Keys.NumPad9:
                    clickedButton = btn9;
                    break;
                case Keys.Oemplus:
                    if (e.Shift)
                    {
                        clickedButton = btnAddition;
                    }
                    else
                    {
                        clickedButton = btnEquals;
                    }
                    break;
                case Keys.Add:
                    clickedButton = btnAddition;
                    break;
                case Keys.OemMinus:
                    if (!e.Shift)
                    {
                        clickedButton = btnSubtraction;
                    }
                    break;
                case Keys.Subtract:
                    clickedButton = btnSubtraction;
                    break;
                case Keys.OemQuestion:
                    clickedButton = btnDivision;
                    break;
                case Keys.Divide:
                    clickedButton = btnDivision;
                    break;
                case Keys.Oemcomma:
                    clickedButton = btnDecimalSeparator;
                    break;
                case Keys.S:
                    clickedButton = btnSin;
                    break;
                case Keys.C:
                    clickedButton = btnCos;
                    break;
                case Keys.L:
                    clickedButton = btnLn;
                    break;
                case Keys.R:
                    clickedButton = btnSquareRoot;
                    break;
                case Keys.Back:
                    clickedButton = btnBackSpace;
                    break;
                case Keys.Enter:
                    clickedButton = btnEquals;
                    break;

            }

            if (clickedButton != null)
            {
                clickedButton.PerformClick();
            }
        }

        private void FormCalc_Shown(object sender, EventArgs e)
        {
            btnEquals.Focus();
        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            const int OFFSET = 46;

            if (isEngineeringMode)
            {
                btnSin.Visible = false;
                btnCos.Visible = false;
                btnLn.Visible = false;
                btnSquareRoot.Visible = false;
                btnDegree.Visible = false;

                this.Location = new Point(this.Location.X + OFFSET, this.Location.Y);
                this.Width = 212;

            }
            else
            {
                btnSin.Visible = true;
                btnCos.Visible = true;
                btnLn.Visible = true;
                btnSquareRoot.Visible = true;
                btnDegree.Visible = true;

                this.Location = new Point(this.Location.X - OFFSET, this.Location.Y);
                this.Width = 258;
            }

            isEngineeringMode = !isEngineeringMode;
            btnEquals.Focus();
        }
    }
}
