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
        public enum TokenType { Number, LeftBracket, RightBracket, Addition, Subtraction, Multiplication, Division, Error };

        public struct Token
        {
            public TokenType tokenType;
            public decimal number;

            public Token() { }

            public Token(TokenType tokenType)
            {
                this.tokenType = tokenType;
            }

            public Token(TokenType tokenType, decimal number)
            {
                this.tokenType = tokenType;
                this.number = number;
            }
        }

        public class Node
        {
            public Token element;
            public Node next;

            public Node() { }

            public Node(Token element)
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

            public void Push(Token element)
            {
                Node newTop = new Node(element);

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

            public Token Pop()
            {
                Token element = new Token();

                if (top != null)
                {
                    element = top.element;
                    top = top.next;
                }
                else
                {
                    element.tokenType = TokenType.Error;
                }

                return element;
            }
        }

        const string ERROR_MESSAGE = "Error";

        bool isDegreeMode = false;
        bool isEngineeringMode = false;

        Stack stack = new Stack();

        static Token[] ConvertExpressionToTokens(string expression)
        {
            Token[] arrayOfTokens = new Token[1000];

            int indexOfCurrentToken = 0;
            char currentChar;
            bool isPreviousCharDigit = false;
            bool isWritingDecimalPlacesMode = false;
            bool isNegativeNumber = false;
            int countDecimalPlaces = 0;

            for (int indexOfCurrentChar = 0; indexOfCurrentChar < expression.Length; indexOfCurrentChar++)
            {
                currentChar = expression[indexOfCurrentChar];

                switch (currentChar)
                {
                    case '(':
                        if (isPreviousCharDigit)
                        {
                            if (isNegativeNumber)
                            {
                                arrayOfTokens[indexOfCurrentToken].number *= -1;
                                isNegativeNumber = false;
                            }

                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken].tokenType = TokenType.LeftBracket;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case ')':
                        if (isPreviousCharDigit)
                        {
                            if (isNegativeNumber)
                            {
                                arrayOfTokens[indexOfCurrentToken].number *= -1;
                                isNegativeNumber = false;
                            }

                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken].tokenType = TokenType.RightBracket;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case '+':
                        if (isPreviousCharDigit)
                        {
                            if (isNegativeNumber)
                            {
                                arrayOfTokens[indexOfCurrentToken].number *= -1;
                                isNegativeNumber = false;
                            }

                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken].tokenType = TokenType.Addition;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case '-':
                        if (isPreviousCharDigit)
                        {
                            if (isNegativeNumber)
                            {
                                arrayOfTokens[indexOfCurrentToken].number *= -1;
                                isNegativeNumber = false;
                            }

                            indexOfCurrentToken += 1;
                        }

                        if (indexOfCurrentChar < expression.Length - 1
                            && (indexOfCurrentToken == 0
                            || arrayOfTokens[indexOfCurrentToken - 1].tokenType == TokenType.LeftBracket))
                        {
                            if (char.IsDigit(expression[indexOfCurrentChar + 1]))
                            {
                                isNegativeNumber = true;
                            }

                            if (expression[indexOfCurrentChar + 1] == '(')
                            {
                                arrayOfTokens[indexOfCurrentToken].tokenType = TokenType.Number;
                                arrayOfTokens[indexOfCurrentToken].number = -1;
                                indexOfCurrentToken++;
                                arrayOfTokens[indexOfCurrentToken].tokenType = TokenType.Multiplication;
                                indexOfCurrentToken++;
                            }
                        }
                        else
                        {
                            arrayOfTokens[indexOfCurrentToken].tokenType = TokenType.Subtraction;
                            indexOfCurrentToken += 1;
                            isPreviousCharDigit = false;
                        }

                        break;

                    case '*':
                        if (isPreviousCharDigit)
                        {
                            if (isNegativeNumber)
                            {
                                arrayOfTokens[indexOfCurrentToken].number *= -1;
                                isNegativeNumber = false;
                            }

                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken].tokenType = TokenType.Multiplication;
                        indexOfCurrentToken += 1;
                        isPreviousCharDigit = false;
                        break;

                    case '/':
                        if (isPreviousCharDigit)
                        {
                            if (isNegativeNumber)
                            {
                                arrayOfTokens[indexOfCurrentToken].number *= -1;
                                isNegativeNumber = false;
                            }

                            indexOfCurrentToken += 1;
                        }
                        arrayOfTokens[indexOfCurrentToken].tokenType = TokenType.Division;
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

                            if (arrayOfTokens[indexOfCurrentToken].number == 0)
                            {
                                arrayOfTokens[indexOfCurrentToken].number = int.Parse(Convert.ToString(currentChar));
                                isPreviousCharDigit = true;
                            }
                            else
                            {
                                arrayOfTokens[indexOfCurrentToken].number = arrayOfTokens[indexOfCurrentToken].number * 10
                                    + int.Parse(Convert.ToString(currentChar));
                            }
                        }

                        if (currentChar == ',')
                        {
                            isWritingDecimalPlacesMode = true;
                        }

                        if (indexOfCurrentChar == expression.Length - 1 || !char.IsDigit(expression[indexOfCurrentChar + 1])
                            && isWritingDecimalPlacesMode)
                        {
                            arrayOfTokens[indexOfCurrentToken].number /= (decimal)Math.Pow(10, countDecimalPlaces);
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

        public bool CheckCorrectness(Token[] arrayOfTokens)
        {
            int currentIndex = 0;
            bool isCorrect = true;

            while (currentIndex < arrayOfTokens.Length && isCorrect)
            {
                if (arrayOfTokens[currentIndex].tokenType == TokenType.LeftBracket)
                {
                    stack.Push(arrayOfTokens[currentIndex]);
                }

                if (arrayOfTokens[currentIndex].tokenType == TokenType.RightBracket)
                {
                    if (stack.top != null && stack.top.element.tokenType == TokenType.LeftBracket)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }

                if (currentIndex == 0
                    && arrayOfTokens[currentIndex].tokenType != TokenType.Number
                    && arrayOfTokens[currentIndex].tokenType != TokenType.LeftBracket)
                {
                    isCorrect = false;
                    break;
                }

                switch (arrayOfTokens[currentIndex].tokenType)
                {
                    case TokenType.LeftBracket:
                        if (!(currentIndex < arrayOfTokens.Length - 2
                            && ((arrayOfTokens[currentIndex + 1].tokenType == TokenType.Number
                            && (int)arrayOfTokens[currentIndex + 2].tokenType >= (int)TokenType.RightBracket)
                            || arrayOfTokens[currentIndex + 1].tokenType == TokenType.LeftBracket)))
                        {
                            isCorrect = false;
                        }
                        break;
                    case TokenType.RightBracket:
                        if (!((currentIndex < arrayOfTokens.Length - 1
                            && (int)arrayOfTokens[currentIndex + 1].tokenType >= (int)TokenType.RightBracket)
                            || currentIndex == arrayOfTokens.Length - 1))
                        {
                            isCorrect = false;
                        }
                        break;
                    default:
                        if ((int)arrayOfTokens[currentIndex].tokenType >= (int)TokenType.Addition)
                        {
                            if (!(currentIndex < arrayOfTokens.Length - 1
                                && (int)arrayOfTokens[currentIndex + 1].tokenType <= (int)TokenType.LeftBracket))
                            {
                                isCorrect = false;
                            }
                        }

                        if (arrayOfTokens[currentIndex].tokenType == TokenType.Number)
                        {
                            if (currentIndex < arrayOfTokens.Length - 1
                                && arrayOfTokens[currentIndex + 1].tokenType == TokenType.LeftBracket)
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

            return isCorrect;
        }

        public Token[] ConvertToReversePolishNotation(Token[] arrayOfTokens)
        {
            int indexOfCurrentTokenInArray = 0;
            int currentIndexInPolishNotation = 0;

            while (indexOfCurrentTokenInArray < arrayOfTokens.Length)
            {

                if (arrayOfTokens[indexOfCurrentTokenInArray].tokenType == TokenType.Number)
                {
                    arrayOfTokens[currentIndexInPolishNotation] = arrayOfTokens[indexOfCurrentTokenInArray];
                    currentIndexInPolishNotation++;
                }
                else
                {
                    if (arrayOfTokens[indexOfCurrentTokenInArray].tokenType == TokenType.LeftBracket)
                    {
                        stack.Push(arrayOfTokens[indexOfCurrentTokenInArray]);
                    }
                    else
                    {
                        if (arrayOfTokens[indexOfCurrentTokenInArray].tokenType == TokenType.RightBracket)
                        {
                            while (stack.top.element.tokenType != TokenType.LeftBracket)
                            {
                                arrayOfTokens[currentIndexInPolishNotation] = stack.Pop();
                                currentIndexInPolishNotation++;
                            }

                            stack.Pop();
                        }
                        else
                        {
                            while (stack.top != null && (int)stack.top.element.tokenType >= (int)arrayOfTokens[indexOfCurrentTokenInArray].tokenType)
                            {
                                arrayOfTokens[currentIndexInPolishNotation] = stack.Pop();
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
                arrayOfTokens[currentIndexInPolishNotation] = stack.Pop();
                currentIndexInPolishNotation++;
            }

            Array.Resize(ref arrayOfTokens, currentIndexInPolishNotation);

            return arrayOfTokens;
        }

        public Token CalculateExpression(Token[] arrayOfTokens)
        {
            Token PerformOperation(TokenType arithmeticOperator, decimal a, decimal b)
            {
                try
                {
                    switch (arithmeticOperator)
                    {
                        case TokenType.Addition: return new Token(TokenType.Number, a + b);
                        case TokenType.Subtraction: return new Token(TokenType.Number, a - b);
                        case TokenType.Multiplication: return new Token(TokenType.Number, a * b);
                        case TokenType.Division: return new Token(TokenType.Number, a / b);
                        default: return new Token(TokenType.Error);
                    }
                }
                catch
                {
                    return new Token(TokenType.Error);
                }
            }

            Token result = new Token(TokenType.Error);
            int currentIndex = 0;
            decimal firstOperand;
            decimal secondOperand;

            while (currentIndex < arrayOfTokens.Length)
            {
                if (arrayOfTokens[currentIndex].tokenType == TokenType.Number)
                {
                    stack.Push(arrayOfTokens[currentIndex]);
                }
                else
                {
                    secondOperand = stack.Pop().number;
                    firstOperand = stack.Pop().number;

                    result = PerformOperation(arrayOfTokens[currentIndex].tokenType, firstOperand, secondOperand);
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
                Token[] arrayOfTokens = ConvertExpressionToTokens(lblExpression.Text);

                if (CheckCorrectness(arrayOfTokens))
                {
                    arrayOfTokens = ConvertToReversePolishNotation(arrayOfTokens);

                    Token result = CalculateExpression(arrayOfTokens);

                    if (result.tokenType != TokenType.Error)
                    {
                        lblExpression.Text = result.number.ToString();
                    }
                    else
                    {
                        lblExpression.Text = ERROR_MESSAGE;
                    }
                }
                else
                {
                    lblExpression.Text = ERROR_MESSAGE;
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
