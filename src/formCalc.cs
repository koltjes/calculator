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
    public partial class formCalc : Form
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

        bool isDegMode = false;
        bool engineerNow = false;

        Stack st = new Stack();

        static decimal[] ConvToArray(string expr)
        {
            decimal[] exprInNum = new decimal[1000];
            int index = 0;
            char el;
            bool prevIsNum = false;
            bool decSep = false;
            int numDecPlaces = 0;

            for (int indExp = 0; indExp < expr.Length; indExp++)
            {
                el = expr[indExp];

                switch (el)
                {
                    case '(':
                        if (prevIsNum)
                        {
                            index += 1;
                        }
                        exprInNum[index] = LEFT;
                        index += 1;
                        prevIsNum = false;
                        break;

                    case ')':
                        if (prevIsNum)
                        {
                            index += 1;
                        }
                        exprInNum[index] = RIGHT;
                        index += 1;
                        prevIsNum = false;
                        break;

                    case '+':
                        if (prevIsNum)
                        {
                            index += 1;
                        }
                        exprInNum[index] = ADD;
                        index += 1;
                        prevIsNum = false;
                        break;

                    case '-':
                        if (prevIsNum)
                        {
                            index += 1;
                        }
                        exprInNum[index] = SUB;
                        index += 1;
                        prevIsNum = false;
                        break;

                    case '*':
                        if (prevIsNum)
                        {
                            index += 1;
                        }
                        exprInNum[index] = MUL;
                        index += 1;
                        prevIsNum = false;
                        break;

                    case '/':
                        if (prevIsNum)
                        {
                            index += 1;
                        }
                        exprInNum[index] = DIV;
                        index += 1;
                        prevIsNum = false;
                        break;

                    default:
                        if (char.IsDigit(el))
                        {
                            if (decSep)
                            {
                                numDecPlaces++;
                            }

                            if (exprInNum[index] == 0)
                            {
                                exprInNum[index] = int.Parse(Convert.ToString(el));
                                prevIsNum = true;
                            }

                            else
                            {
                                exprInNum[index] = exprInNum[index] * 10 + int.Parse(Convert.ToString(el));
                            }
                        }

                        if (el == ',')
                        {
                            decSep = true;
                        }

                        if ((indExp == expr.Length - 1 || !char.IsDigit(expr[indExp + 1])) && decSep)
                        {
                            exprInNum[index] /= (decimal)Math.Pow(10, numDecPlaces);
                            numDecPlaces = 0;
                            decSep = false;
                        }

                        break;
                }
            }

            if (prevIsNum)
            {
                index += 1;
            }

            Array.Resize(ref exprInNum, index);

            return exprInNum;
        }

        public bool Correctness(decimal[] exprInNum)
        {
            int index = 0;
            bool correct = true;

            while (index < exprInNum.Length && correct)
            {
                if (exprInNum[index] == LEFT)
                {
                    st.Push(LEFT);
                }

                if (exprInNum[index] == RIGHT)
                {
                    if (st.top != null && st.top.element == LEFT)
                    {
                        st.Pop();
                    }

                    else
                    {
                        correct = false;
                    }
                }

                if (index == 0 && !(exprInNum[index] >= -1))
                {
                    correct = false;
                    break;
                }

                switch (exprInNum[index])
                {
                    case LEFT:
                        if (!(index < exprInNum.Length - 2 && ((exprInNum[index + 1] >= 0 && exprInNum[index + 2] <= ADD) | exprInNum[index + 1] == LEFT)))
                        {
                            correct = false;
                        }
                        break;
                    case RIGHT:
                        if (!((index < exprInNum.Length - 1 && exprInNum[index + 1] <= RIGHT) | index == exprInNum.Length - 1))
                        {
                            correct = false;
                        }
                        break;
                    default:
                        if (exprInNum[index] <= -3)
                        {
                            if (!(index < exprInNum.Length - 1 && exprInNum[index + 1] >= LEFT))
                            {
                                correct = false;
                            }
                        }

                        if (exprInNum[index] >= 0)
                        {
                            if (index < exprInNum.Length - 1 && exprInNum[index + 1] == LEFT)
                            {
                                correct = false;
                            }
                        }
                        break;
                }

                index++;
            }

            if (st.top != null)
            {
                correct = false;
            }

            if (!correct)
            {
                lblExpr.Text = "Error";
            }

            return correct;
        }

        public decimal[] ConvertToPolish(decimal[] exprInNum)
        {
            int index = 0;
            int indexPolish = 0;

            while (index < exprInNum.Length)
            {
                if (exprInNum[index] >= 0)
                {
                    exprInNum[indexPolish] = exprInNum[index];
                    indexPolish++;
                }

                else
                {
                    if (exprInNum[index] == LEFT)
                    {
                        st.Push(exprInNum[index]);
                    }

                    else
                    {
                        if (exprInNum[index] == RIGHT)
                        {
                            while (st.top.element != LEFT)
                            {
                                exprInNum[indexPolish] = Convert.ToInt32(st.Pop());
                                indexPolish++;
                            }

                            st.Pop();
                        }

                        else
                        {
                            while (st.top != null && st.top.element <= exprInNum[index])
                            {
                                exprInNum[indexPolish] = Convert.ToInt32(st.Pop());
                                indexPolish++;
                            }

                            st.Push(exprInNum[index]);
                        }
                    }
                }

                index++;
            }

            while (st.top != null)
            {
                exprInNum[indexPolish] = Convert.ToInt32(st.Pop());
                indexPolish++;
            }

            Array.Resize(ref exprInNum, indexPolish);

            return exprInNum;
        }

        public decimal Calculate(decimal[] exprInNum)
        {
            decimal Action(decimal oper, decimal a, decimal b)
            {
                try
                {
                    switch (oper)
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

            decimal res = ERR;
            int index = 0;
            decimal op1;
            decimal op2;

            while (index < exprInNum.Length)
            {
                if (exprInNum[index] >= 0)
                {
                    st.Push(exprInNum[index]);
                }

                else
                {
                    op2 = Convert.ToDecimal(st.Pop());
                    op1 = Convert.ToDecimal(st.Pop());

                    res = Action(exprInNum[index], op1, op2);
                    st.Push(res);
                }

                index++;
            }

            st.Pop();

            return res;
        }

        public formCalc()
        {
            InitializeComponent();
        }

        private void formCalc_Load(object sender, EventArgs e)
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
            btnSum.Click += new EventHandler(input_Click);
            btnSub.Click += new EventHandler(input_Click);
            btnMul.Click += new EventHandler(input_Click);
            btnDiv.Click += new EventHandler(input_Click);
            btnLeftBr.Click += new EventHandler(input_Click);
            btnRightBr.Click += new EventHandler(input_Click);
            btnDecSep.Click += new EventHandler(input_Click);
            btnDeg.Click += new EventHandler(input_Click);

            btnSin.Visible = false;
            btnCos.Visible = false;
            btnLn.Visible = false;
            btnSqrt.Visible = false;
            btnDeg.Visible = false;

            this.Width = 212;
        }

        private void input_Click(object sender, EventArgs e)
        {
            var btnNum = (Button)sender;

            if ((lblExpr.Text == "0" || lblExpr.Text == "Error") && btnNum.Text != "," && btnNum.Text != "^")
            {
                lblExpr.Text = btnNum.Text;
            }
            else
            {
                lblExpr.Text += btnNum.Text;
            }

            btnEq.Focus();
        }

        private void btnBackSp_Click(object sender, EventArgs e)
        {
            if (lblExpr.Text.Length > 1 && lblExpr.Text != "Error")
            {
                lblExpr.Text = lblExpr.Text.Remove(lblExpr.Text.Length - 1);
            }
            else
            {
                lblExpr.Text = "0";
            }

            btnEq.Focus();
        }

        private void btnEq_Click(object sender, EventArgs e)
        {
            if (isDegMode)
            {
                try
                {
                    double[] degr = lblExpr.Text.Split('^').Select(double.Parse).ToArray();

                    if (degr.Length == 2)
                    {
                        double res = Math.Pow(degr[0], degr[1]);
                        lblExpr.Text = res.ToString();
                    }
                    else
                    {
                        lblExpr.Text = "Error";
                    }
                }

                catch
                {
                    lblExpr.Text = "Error";
                }

            }
            else
            {
                decimal[] exprInNum = ConvToArray(lblExpr.Text);

                if (Correctness(exprInNum))
                {
                    exprInNum = ConvertToPolish(exprInNum);

                    decimal res = Calculate(exprInNum);

                    if (res != ERR)
                    {
                        lblExpr.Text = res.ToString();
                    }
                    else
                    {
                        lblExpr.Text = "Error";
                    }
                }
            }
        }

        private double ConvDegToRadians(double angle)
        {
            angle = angle * Math.PI / 180;
            return angle;
        }

        private void btnSin_Click(object sender, EventArgs e)
        {
            try
            {
                double res = Math.Sin(ConvDegToRadians(Convert.ToDouble(lblExpr.Text)));
                lblExpr.Text = res.ToString();
            }

            catch
            {
                lblExpr.Text = "Error";
            }
            finally
            {
                btnEq.Focus();
            }
        }

        private void btnCos_Click(object sender, EventArgs e)
        {
            try
            {
                double res = Math.Cos(ConvDegToRadians(Convert.ToDouble(lblExpr.Text)));
                lblExpr.Text = res.ToString();
            }

            catch
            {
                lblExpr.Text = "Error";
            }
            finally
            {
                btnEq.Focus();
            }
        }

        private void btnLn_Click(object sender, EventArgs e)
        {
            try
            {
                double res = Math.Log(Convert.ToDouble(lblExpr.Text));
                lblExpr.Text = res.ToString();
            }

            catch
            {
                lblExpr.Text = "Error";
            }
            finally
            {
                btnEq.Focus();
            }
        }

        private void btnSqrt_Click(object sender, EventArgs e)
        {
            try
            {
                double res = Math.Sqrt(Convert.ToDouble(lblExpr.Text));
                lblExpr.Text = res.ToString();
            }

            catch
            {
                lblExpr.Text = "Error";
            }
            finally
            {
                btnEq.Focus();
            }
        }

        private void btnDeg_Click(object sender, EventArgs e)
        {
            isDegMode = true;

            btnEq.Focus();
        }

        private void formCalc_KeyDown(object sender, KeyEventArgs e)
        {
            Button btn = null;

            switch (e.KeyCode)
            {
                case Keys.D0:
                    if (e.Shift)
                    {
                        btn = btnRightBr;
                    }
                    else
                    {
                        btn = btn0;
                    }
                    break;
                case Keys.NumPad0:
                    btn = btn0;
                    break;
                case Keys.D1:
                    btn = btn1;
                    break;
                case Keys.NumPad1:
                    btn = btn1;
                    break;
                case Keys.D2:
                    btn = btn2;
                    break;
                case Keys.NumPad2:
                    btn = btn2;
                    break;
                case Keys.D3:
                    btn = btn3;
                    break;
                case Keys.NumPad3:
                    btn = btn3;
                    break;
                case Keys.D4:
                    btn = btn4;
                    break;
                case Keys.NumPad4:
                    btn = btn4;
                    break;
                case Keys.D5:
                    btn = btn5;
                    break;
                case Keys.NumPad5:
                    btn = btn5;
                    break;
                case Keys.D6:
                    if (e.Shift)
                    {
                        btn = btnDeg;
                    }
                    else
                    {
                        btn = btn6;
                    }
                    break;
                case Keys.NumPad6:
                    btn = btn6;
                    break;
                case Keys.D7:
                    btn = btn7;
                    break;
                case Keys.NumPad7:
                    btn = btn7;
                    break;
                case Keys.D8:
                    if (e.Shift)
                    {
                        btn = btnMul;
                    }
                    else
                    {
                        btn = btn8;
                    }
                    break;
                case Keys.NumPad8:
                    btn = btn8;
                    break;
                case Keys.Multiply:
                    btn = btnMul;
                    break;
                case Keys.D9:
                    if (e.Shift)
                    {
                        btn = btnLeftBr;
                    }
                    else
                    {
                        btn = btn9;
                    }
                    break;
                case Keys.NumPad9:
                    btn = btn9;
                    break;
                case Keys.Oemplus:
                    if (e.Shift)
                    {
                        btn = btnSum;
                    }
                    else
                    {
                        btn = btnEq;
                    }
                    break;
                case Keys.Add:
                    btn = btnSum;
                    break;
                case Keys.OemMinus:
                    if (!e.Shift)
                    {
                        btn = btnSub;
                    }
                    break;
                case Keys.Subtract:
                    btn = btnSub;
                    break;
                case Keys.OemQuestion:
                    btn = btnDiv;
                    break;
                case Keys.Divide:
                    btn = btnDiv;
                    break;
                case Keys.Oemcomma:
                    btn = btnDecSep;
                    break;
                case Keys.S:
                    btn = btnSin;
                    break;
                case Keys.C:
                    btn = btnCos;
                    break;
                case Keys.L:
                    btn = btnLn;
                    break;
                case Keys.R:
                    btn = btnSqrt;
                    break;
                case Keys.Back:
                    btn = btnBackSp;
                    break;
                case Keys.Enter:
                    btn = btnEq;
                    break;

            }

            if (btn != null)
            {
                btn.PerformClick();
            }
        }

        private void formCalc_Shown(object sender, EventArgs e)
        {
            btnEq.Focus();
        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            if (engineerNow)
            {
                btnSin.Visible = false;
                btnCos.Visible = false;
                btnLn.Visible = false;
                btnSqrt.Visible = false;
                btnDeg.Visible = false;

                this.Location = new Point(this.Location.X + 46, this.Location.Y);
                this.Width = 212;

            }
            else
            {
                btnSin.Visible = true;
                btnCos.Visible = true;
                btnLn.Visible = true;
                btnSqrt.Visible = true;
                btnDeg.Visible = true;

                this.Location = new Point(this.Location.X - 46, this.Location.Y);
                this.Width = 258;
            }

            engineerNow = !engineerNow;
            btnEq.Focus();
        }
    }
}
