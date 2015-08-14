/*
 * Name: Tyler Cruz
 * ID:  11333476
 * 
 * Description: 
 * 
 * HW10
 * Deal with circular references
 * 
 * Progress towards an app similar to excel
 * Enter text into gui cell and the text will be process by the Spreadsheet Engine
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEngine
{
    class Expression
    {
        Node m_root;
        Dictionary<string, double> m_dict = new Dictionary<string, double>();
        public HashSet<string> m_keys = new HashSet<string>();

        public Expression(string expression)
        {
            if (expression != "" && expression != null)
            {
                m_root = makeExprTree(expression);
            }
            else
            {

                m_root = null;
            }
        }

        private abstract class Node
        {

        }

        private class ValueNode : Node
        {
            public double m_value;//value nodes store a constant double value
        }

        private class VarNode : Node
        {
            public string m_var;//variable nodes store the string of a variable name
        }

        private class OpNode : Node
        {
            public char m_op;//store an operator character
            //should always have non-null left and right nodes
            public Node m_left;
            public Node m_right;
        }

        private Node makeExprTree(string s)
        {
            char[] ops = { '+', '-', '*', '/', '^' };//in order of precedence

            int parenCount = 0;

            foreach (char op in ops)
            {
                for (int i = s.Length - 1; i >= 0; i--)//go from back to front (find least precedent op first)
                {
                    int index = 0;

                    if (op == '^')//adjust index to go from right to left since '^' is right associative
                    {
                        index = s.Length - 1 - i;
                    }
                    else
                    {
                        index = i;
                    }

                    if (s[index] == '(')
                    {
                        parenCount++;
                    }
                    else if (s[index] == ')')
                    {
                        parenCount--;
                    }

                    if (parenCount == 0)//we don't care about ops nested in parentheses
                    {
                        if (op == s[index])//if we run into an op, add an op node to the tree
                        {
                            return new OpNode()
                            {  //we'll have problems is an op is not placed in the correct position
                                m_op = op,
                                //recursively call this function on left an right substrings, to create left and right children
                                m_left = makeExprTree(s.Substring(0, index)),
                                m_right = makeExprTree(s.Substring(index + 1)),
                            };
                        }
                    }
                    //if parenCount < 0 parentheses are off balance
                }
            }

            //if we get to this point, the substring no longer has any op nodes in it

            try//see if string can be converted to double, if so, add value node
            {
                Convert.ToDouble(s);
                return new ValueNode()//add value node
                {
                    m_value = Convert.ToDouble(s)
                };
            }
            catch//otherwise string must be a variable name
            {
                removeSpaces(ref s);//remove spaces from variable

                //check to see if parentheses surround this string.
                //if surrounded by parens, we need to call this function on a sub string that exclude the outermost parentheses
                if (s[0] == '(')
                {
                    if (s.Length >= 3)
                    {
                        if (s[s.Length - 1] == ')')//if we have parens at front and end
                        {
                            return makeExprTree(s.Substring(1, s.Length - 2));
                        }
                        //throw exception here 
                    }//if less we have some kind of parent unbalance maybe () or (3
                    //throw exception for parentheses being imbalanced
                }
                else if (s[s.Length - 1] == ')')//to get this perfect there are many possibilities for this error
                {
                    //throw exception, unbalanced parentheses

                }

                m_keys.Add(s);
                m_dict[s] = 0;//default each variable to 0

                return new VarNode()//add variable node
                {
                    m_var = s
                };
            }
        }

        private double evalNode(Node node)
        {
            OpNode op_node = node as OpNode;
            ValueNode val_node = node as ValueNode;
            VarNode var_node = node as VarNode;

            if (null != op_node)
            {
                switch (op_node.m_op)
                {
                    case '+': return evalNode(op_node.m_left) + evalNode(op_node.m_right);
                    case '-': return evalNode(op_node.m_left) - evalNode(op_node.m_right);
                    case '*': return evalNode(op_node.m_left) * evalNode(op_node.m_right);
                    case '/': return evalNode(op_node.m_left) / evalNode(op_node.m_right);
                    case '^': return Math.Pow(evalNode(op_node.m_left), evalNode(op_node.m_right));
                }
            }

            if (null != val_node)//if node is a value node, return it's value
            {
                return val_node.m_value;
            }

            if (null != var_node)//if node is a variable, look up key in dictionary and return corresponding value
            {
                return m_dict[var_node.m_var];//will crash if variable isn't defined, so don't allows eval function to be called unless all variables are define. SEE allVarsDefined() function
            }

            return 0;//I don't know the how this code could get here, but the compiler wants something here
        }

        public double evalTree()
        {
            return evalNode(m_root);
        }

        public void define(string key, double value)//pass in 
        {
            //if (m_keys.Contains(key))//if variable exists in tree, define it.
            //{
            m_dict[key] = value;
            //}
        }

        //public double getValueOf(string key)
        //{
        //   return m_dict[key];
        //}

        void removeSpaces(ref string s)
        {
            string new_string = "";

            for (int i = 0; i < s.Length; i++)//remove space if there are any
            {
                if (s[i] != ' ')
                {
                    new_string += s[i];
                }
            }

            s = new_string;
        }

        bool allVarsDefinedNode(Node node)
        {
            OpNode o = node as OpNode;
            if (o != null)//node is an OpNode
            {
                return allVarsDefinedNode(o.m_left) && allVarsDefinedNode(o.m_right);
            }

            ValueNode value = node as ValueNode;
            if (value != null)//node is a ValueNode
            {
                return true;
            }

            VarNode var = node as VarNode;
            if (var != null)//node is a VarNode
            {
                if (m_dict.ContainsKey(var.m_var))//if key exists
                {
                    return true;
                }
                else//if key is not in dictionary
                {
                    return false;
                }
            }

            return false;//Don't know how code would get here
        }

        public bool allVarsDefined()
        {
            return allVarsDefinedNode(m_root);
        }

        //make a validate variable name function

        public void printVariables()//print variables in the tree and their corresponding value
        {
            foreach (string key in m_keys)
            {
                if (m_dict.ContainsKey(key))//if var is defined in dictionary
                {
                    Console.WriteLine(key + ": " + m_dict[key]);
                }
                else
                {
                    Console.WriteLine(key + ": UNDEFINED");//variable is defined
                }
            }
        }
    }
}
