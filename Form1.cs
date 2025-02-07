using System.Collections.Concurrent;
using System.Diagnostics;
using static System.Windows.Forms.Design.AxImporter;

namespace T7_8._0_
{
    public partial class Form1 : Form
    {
        public Form1() => InitializeComponent();

        static List<Button[]> buttons = [];
        static Problem? problem;
        static int boardSize = 8;

        public void UpdateUI()
        {
            buttons = [];

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    Button? button = (Button?)Controls[$"Button{j + 1}_{i + 1}"];
                    //if(button != null)
                    button.Visible = false;
                }
            //if (buttons != null)
            //{
            for (int i = 0; i < boardSize; i++)
            {
                Button[] buttonsRow = new Button[boardSize];
                buttons.Add(buttonsRow);
                for (int j = 0; j < boardSize; j++)
                {
                    Button? button = (Button?)Controls[$"Button{j + 1}_{i + 1}"];
                    //if (button != null)
                    //{
                    button.Visible = true;
                    button.FlatAppearance.BorderSize = 0;
                    buttons.ElementAt(i)[j] = button;
                    //}
                }
            }
            problem = new Problem(buttons);
            //}
            NewButton.PerformClick();
            Update();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //👑
            //♕
            UpdateUI();
        }

        static byte[] initialQueenPositionArray = new byte[boardSize];

        class State
        {
            public List<Button[]> buttons = [];
            public State(List<Button[]> buttons)
            {
                for (int i = 0; i < boardSize; i++)
                {
                    this.buttons.Add(new Button[boardSize]);
                    for (int j = 0; j < boardSize; j++)
                        this.buttons.ElementAt(i)[j] = buttons.ElementAt(i)[j];
                }
            }
        }

        class Node(State state, Node? parent, Tuple<int, int> actionClause,
        //int? pathCost, 
        int heuristic)
        {
            public State state = state;
            public Node? parent = parent;
            public Tuple<int, int> actionClause = actionClause;
            //int? pathCost, 
            public int heuristic = heuristic;
        }

        class Problem
        {

            //bool isFinished = false;
            private static List<Button[]> MoveHandler(int action, int col, List<Button[]> btns)
            {
                List<Button[]> clonedBtns = [];
                for (int i = 0; i < boardSize; i++)
                {
                    clonedBtns.Add(new Button[boardSize]);
                    for (int j = 0; j < boardSize; j++)
                        clonedBtns.ElementAt(i)[j] = new Button
                        {
                            Text = btns.ElementAt(i)[j].Text
                        };
                }

                byte[] clonedQueenPositionArray = new byte[boardSize];

                for (int i = 0; i < boardSize; i++)
                    for (int j = 0; j < boardSize; j++)
                        if (btns.ElementAt(i)[j].Text == "♕")
                            clonedQueenPositionArray[i] = (byte)j;

                int fullButtonIndex = clonedQueenPositionArray[col];

                clonedBtns.ElementAt(col)[action].Text = clonedBtns.ElementAt(col)[fullButtonIndex].Text;
                clonedBtns.ElementAt(col)[fullButtonIndex].Text = "";

                return clonedBtns;
            }
            public State InitialState;
            public State CompleteState;

            public Problem(List<Button[]> buttons)
            {
                this.InitialState = new State(buttons);

                List<Button[]> completeBtns = [];

                for (int i = 0; i < boardSize; i++)
                {
                    completeBtns.Add(new Button[boardSize]);
                    for (int j = 0; j < boardSize; j++)
                        completeBtns[i][j] = buttons[i][j];

                }
                CompleteState = new State(completeBtns);
            }
            public static State Result(State state, int col, int action)
            {
                List<Button[]> buttons = MoveHandler(action, col, state.buttons);
                State resultState = new(buttons);
                return resultState;
            }

            public static bool GoalTest(State state)
            {
                byte[] clonedQueenPositionArray = new byte[boardSize];

                for (int i = 0; i < boardSize; i++)
                    for (int j = 0; j < boardSize; j++)
                        if (state.buttons.ElementAt(i)[j].Text == "♕")
                            clonedQueenPositionArray[i] = (byte)j;



                bool result = true;
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = i + 1; j < boardSize; j++)
                        if (clonedQueenPositionArray[i] == clonedQueenPositionArray[j] ||
                            Math.Abs(clonedQueenPositionArray[i] - clonedQueenPositionArray[j]) == Math.Abs(i - j))
                        {
                            result = false;
                            break;
                        }
                    if (!result)
                        break;
                }
                return result;
            }

            public static List<Tuple<int, int>> Actions(State state)
            {
                List<Tuple<int, int>> res = [];

                int[] actionsArray = new int[boardSize];
                for (int i = 0; i < boardSize; i++)
                    actionsArray[i] = i;

                for (int i = 0; i < boardSize; i++)
                    foreach (int action in actionsArray)
                        if (state.buttons.ElementAt(i)[action].Text != "♕")
                            res.Add(new Tuple<int, int>(i, action));

                return res;
            }
        }
        private void HideComponentsBeforeSolving(string[] options)
        {
            if (options.Contains("clear-list"))
                resultListBox.Items.Clear();
            solvedLabel.Visible = false;
            if (options.Contains("actions"))
                actionsLabel.Visible = false;
            if (options.Contains("time"))
                timeLabel.Visible = false;
            if (options.Contains("list"))
                resultListBox.Visible = false;
        }
        private void DisplayComponentsAfterSolving(Stopwatch stopwatch, string[] options)
        {
            actionsLabel.Text = "Actions Count: " + resultListBox.Items.Count.ToString();
            timeLabel.Text = "Time: " + stopwatch.ElapsedMilliseconds.ToString() + " milliseconds";
            //resultListBox.Items.Clear();
            if (options.Contains("actions"))
                actionsLabel.Visible = true;
            if (options.Contains("time"))
                timeLabel.Visible = true;
            if (options.Contains("list"))
                resultListBox.Visible = true;
        }

        private void WinCheck()
        {
            bool isSolved = Problem.GoalTest(new State(buttons));
            if (isSolved)
            {
                solvedLabel.Visible = true;
            }
        }

        //Hill Climbing:===============================================================
        private static int HeuristicCalculator(State current)
        {
            byte[] clonedQueenPositionArray = new byte[boardSize];

            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++)
                    if (current.buttons.ElementAt(i)[j].Text == "♕")
                        clonedQueenPositionArray[i] = (byte)j;

            int cost = 0;
            for (int i = 0; i < boardSize; i++)
                for (int j = i + 1; j < boardSize; j++)
                    if (clonedQueenPositionArray[i] == clonedQueenPositionArray[j] ||
                            Math.Abs(clonedQueenPositionArray[i] - clonedQueenPositionArray[j]) == Math.Abs(i - j))
                    {
                        cost++;
                    }
            return cost;
        }
        private static Node ChildNode(Node parent, int col, int action)
        {
            State resultState = Problem.Result(parent.state, col, action);
            return new Node(
                resultState,
                parent,
                new Tuple<int, int>(col, action),
                HeuristicCalculator(resultState)
                );
        }
        class HillClimbingCompare : IComparer<Node>
        {
            public int Compare(Node? x, Node? y)
            {
                //if (x != null && y != null)
                // CompareTo() method 
                return x.heuristic.CompareTo(y.heuristic);
                //return 0;
            }
        }
        private string HillClimb()
        {
            Stack<Node> myStack = new();
            problem = new Problem(buttons);
            State state = problem.InitialState;

            Stopwatch sw = new();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                if (Problem.GoalTest(state))
                {
                    for (int i = 0; i < boardSize; i++)
                        for (int j = 0; j < boardSize; j++)
                            buttons.ElementAt(i)[j].Text = state.buttons.ElementAt(i)[j].Text;

                    return "success";
                }
                else
                {
                    List<Node> successors = [];
                    foreach (var actionClause in Problem.Actions(state))
                    {
                        int col = actionClause.Item1;
                        int action = actionClause.Item2;
                        successors.Add(ChildNode(new Node(state, null, actionClause, HeuristicCalculator(state)), col, action));
                        HillClimbingCompare mc = new();
                        successors.Sort(mc);
                    }
                    successors.Reverse();
                    foreach (var successor in successors)
                        myStack.Push(successor);
                }
                if (myStack.Count == 0)
                    return "failure";
                resultListBox.Items.Add($"{myStack.Peek().actionClause.Item1} ; {myStack.Peek().actionClause.Item2}");
                state = myStack.Pop().state;
            }
            sw.Stop();
            return "failure";
        }
        private void HillClimbingButton_Click(object sender, EventArgs e)
        {
            HideComponentsBeforeSolving(["actions", "clear-list", "time", "list"]);

            Stopwatch sw = new();
            sw.Start();
            _ = HillClimb();
            sw.Stop();

            DisplayComponentsAfterSolving(sw, ["time", "actions", "list"]);

            WinCheck();
            //MessageBox.Show(result);
        }

        //Genetic:===============================================================
        private static int maxConflicts = boardSize * (boardSize - 1) / 2;
        private static int GeneticFitnessCalculator(byte[] currentQueenPositionArray)
        {

            int fitness = 0;
            for (int i = 0; i < boardSize; i++)
                for (int j = i + 1; j < boardSize; j++)
                {
                    if (currentQueenPositionArray[i] != currentQueenPositionArray[j] &&
                            Math.Abs(currentQueenPositionArray[i] - currentQueenPositionArray[j]) != Math.Abs(i - j))
                        //if (currentQueenPositionArray[i] == currentQueenPositionArray[j] ||
                        //        Math.Abs(currentQueenPositionArray[i] - currentQueenPositionArray[j]) == Math.Abs(i - j))
                        fitness++;

                    //if (currentQueenPositionArray[i] == currentQueenPositionArray[j])
                    //    fitness++;
                    //if (Math.Abs(currentQueenPositionArray[i] - currentQueenPositionArray[j]) == Math.Abs(i - j))
                    //    fitness++;
                }
            //return maxConflicts - fitness;
            return fitness;
            //return -fitness;
        }
        //class GeneticCompare : IComparer<byte[]>
        //{
        //    public int Compare(byte[]? x, byte[]? y)
        //    {
        //        //if (x != null && y != null)
        //        // CompareTo() method 
        //        return GeneticFitnessCalculator(x).CompareTo(GeneticFitnessCalculator(y));
        //        //return 0;
        //    }
        //}
        private static byte[] RandomSelection(List<byte[]> population)
        {
            byte[] heuristics = new byte[population.Count];
            //List<byte> heuristics = [];
            byte[] selectProbability = new byte[population.Count];
            //List<byte[]> sortedPopulation = [];
            int sum = 0;
            //for (int i = 0; i < population.Count; i++)
            //    sortedPopulation.Add(population[i]);

            //GeneticCompare gc = new();
            //sortedPopulation.Sort(gc);
            //sortedPopulation.Reverse();


            for (int i = 0; i < population.Count; i++)
            {
                heuristics[i] = (byte)GeneticFitnessCalculator(population.ElementAt(i));
                //heuristics[i] = (byte)(i + 1);
                sum += heuristics[i];
            }


            for (int i = 0; i < population.Count; i++)
                if (i == 0)
                    selectProbability[i] = (byte)(heuristics[i] / Convert.ToDouble(sum) * 100);
                else if (i == population.Count - 1)
                    selectProbability[i] = 100;
                else
                {
                    selectProbability[i] = (byte)(heuristics[i] / Convert.ToDouble(sum) * 100);
                    selectProbability[i] += selectProbability[i - 1];
                }


            byte[] resultArray = new byte[boardSize];
            int randomValue = new Random().Next(100);
            for (int i = 0; i < population.Count; i++)
                if (randomValue < selectProbability[i])
                {
                    resultArray = population.ElementAt(i);
                    //resultArray = sortedPopulation.ElementAt(i);
                    break;
                }


            return resultArray;
        }
        private static byte[] Reproduce(byte[] x, byte[] y)
        {
            byte[] resultArray = new byte[boardSize];

            byte index = (byte)new Random().Next(boardSize);
            for (int i = 0; i < index; i++)
            {
                resultArray[i] = x[i];
            }
            for (int i = index; i < boardSize; i++)
            {
                resultArray[i] = y[i];
            }

            return resultArray;
        }
        private static byte[] Mutate(byte[] currentQueenPositionArray)
        {
            Random rn = new();
            byte index = (byte)rn.Next(currentQueenPositionArray.Length);
            byte index2 = index;
            while (index == index2)
                index2 = (byte)rn.Next(currentQueenPositionArray.Length);
            currentQueenPositionArray[index] = (byte)rn.Next(boardSize);
            currentQueenPositionArray[index2] = (byte)rn.Next(boardSize);
            //currentQueenPositionArray[index + 1] = (byte)rn.Next(boardSize);
            return currentQueenPositionArray;
        }

        //private static int initialPopulationCount = 2;
        //private static int mutationProbabilityPercent = 77;
        private static readonly int initialPopulationCount = 2;
        private static readonly int mutationProbabilityPercent = 77;
        private static byte[] Genetic(List<byte[]> population)
        {
            byte[] resultArray = population.ElementAt(0);
            bool isFound = false;

            Stopwatch sw = new();
            sw.Start();
            int generationsCount = 0;
            while (
                sw.ElapsedMilliseconds < 10000 && generationsCount < 1000000 &&
                !isFound)
            {
                List<byte[]> new_population = [];
                for (int i = 0; i < population.Count; i++)
                {
                    byte[] x = RandomSelection(population);
                    byte[] y = RandomSelection(population);
                    byte[] child = Reproduce(x, y);
                    if (new Random().Next(100) < mutationProbabilityPercent)
                    {
                        child = Mutate(child);
                    }
                    if (GeneticFitnessCalculator(child) == maxConflicts)
                    //if (GeneticFitnessCalculator(child) == 0)
                    {
                        isFound = true;
                        //resultArray = child;
                        child.CopyTo(resultArray, 0);
                    }
                    new_population.Add(child);
                }
                generationsCount++;

                population.Clear();
                for (int i = 0; i < initialPopulationCount; i++)
                    population.Add(new_population.ElementAt(i));

            }
            sw.Stop();

            List<int> Heuristics = [];
            foreach (byte[] individual in population)
                Heuristics.Add(GeneticFitnessCalculator(individual));

            if (!isFound)
            {
                int resultIndex = Heuristics.IndexOf(Heuristics.Max());
                //MessageBox.Show("Not found.\nMax heuristic: " + Heuristics.Max() + "\nTime: " + sw.ElapsedMilliseconds
                //    + "\nGenerations Count: " + generationsCount);
                resultArray = population.ElementAt(resultIndex);
                //resultListBox.Items.Add("Not found.; " + Heuristics.Max() + "; " + sw.ElapsedMilliseconds
                //    + "; " + generationsCount);

                return resultArray;
            }

            //string s = "Found!\nResult array: ";
            //string s = "Found!; ";
            //foreach (byte individual in resultArray)
            //    s += individual.ToString() + ";";
            //s += "\nMax heuristic: " + Heuristics.Max().ToString();
            //s += "\nTime: " + sw.ElapsedMilliseconds;
            //s += "\nGenerations Count: " + generationsCount;
            //MessageBox.Show(s);

            //s += "; " + Heuristics.Max().ToString();
            //s += "; " + sw.ElapsedMilliseconds;
            //s += "; " + generationsCount;
            //resultListBox.Items.Add(s);

            return resultArray;
        }

        //private void CompTest(ref string result)
        //{
        //    long totalTime = 0;
        //    Stopwatch sw;
        //    byte count = 25;
        //    Random rn = new();

        //    if (initialPopulationCount == -1)
        //        initialPopulationCount = 2;
        //    if (mutationProbabilityPercent == -1)
        //        mutationProbabilityPercent = (short)rn.Next(77, 78);
        //    for (int i = 0; i < count; i++)
        //    {
        //        List<byte[]> initialPopulation = [];
        //        for (int z = 0; z < initialPopulationCount; z++)
        //        {
        //            NewButton.PerformClick();
        //            byte[] tempQueenPositionArray = new byte[boardSize];
        //            for (int j = 0; j < boardSize; j++)
        //            {
        //                tempQueenPositionArray[j] = initialQueenPositionArray[j];
        //            }
        //            initialPopulation.Add(tempQueenPositionArray);
        //        }
        //        sw = new();
        //        sw.Start();
        //        _ = Genetic(initialPopulation);
        //        sw.Stop();
        //        totalTime += sw.ElapsedMilliseconds;
        //    }
        //    result += ($"\ntotal time for {count} solve: {totalTime}\t population:{initialPopulationCount}\t mutation:{mutationProbabilityPercent}");
        //}
        //private void Comp()
        //{
        //    string result = "";
        //    initialPopulationCount = 2;
        //    mutationProbabilityPercent = 77;
        //    CompTest(ref result);
        //    initialPopulationCount = -1;
        //    mutationProbabilityPercent = -1;
        //    CompTest(ref result);

        //    MessageBox.Show(result);
        //}

        private void GeneticButton_Click(object sender, EventArgs e)
        {
            HideComponentsBeforeSolving(["actions", "time", "list"]);

            //Comp();

            List<byte[]> initialPopulation = [];
            for (int i = 0; i < initialPopulationCount; i++)
            {
                NewButton.PerformClick();
                byte[] tempQueenPositionArray = new byte[boardSize];
                for (int j = 0; j < boardSize; j++)
                {
                    tempQueenPositionArray[j] = initialQueenPositionArray[j];
                }
                initialPopulation.Add(tempQueenPositionArray);
            }

            Stopwatch sw = new();
            sw.Start();
            byte[] resultArray = Genetic(initialPopulation);
            sw.Stop();

            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++)
                {
                    if (j == resultArray[i])
                        buttons.ElementAt(i)[j].Text = "♕";
                    else
                        buttons.ElementAt(i)[j].Text = "";
                }

            DisplayComponentsAfterSolving(sw, ["time"]);

            WinCheck();
        }

        //CSP:===============================================================
        private static byte CSPFitnessCalculator(byte[] currentQueenPositionArray)
        {
            byte fitness = 0;
            for (int i = 0; i < boardSize; i++)
                for (int j = i + 1; j < boardSize; j++)
                    if (currentQueenPositionArray[i] != 100 && currentQueenPositionArray[j] != 100 &&
                        currentQueenPositionArray[i] != currentQueenPositionArray[j] &&
                            Math.Abs(currentQueenPositionArray[i] - currentQueenPositionArray[j]) != Math.Abs(i - j))
                        //if (currentQueenPositionArray[i] == currentQueenPositionArray[j] ||
                        //        Math.Abs(currentQueenPositionArray[i] - currentQueenPositionArray[j]) == Math.Abs(i - j))
                        fitness++;       
            return fitness;
        }
        private static byte count = 0;
        private static bool IsConsistent(byte value, byte index, byte[] assignment)
        {
            assignment[index] = value;

            count = 0;
            for (int i = 0; i < assignment.Length; i++)
                if (assignment[i] != 100)
                    count++;
            if (CSPFitnessCalculator(assignment) == (count * (count - 1) / 2))
                return true;
            else return false;
        }
        class CSPCompare : IComparer<byte[]>
        {
            public int Compare(byte[]? x, byte[]? y)
            {
                //if (x != null && y != null)
                // CompareTo() method 
                return CSPFitnessCalculator(x).CompareTo(CSPFitnessCalculator(y));
                //return 0;
            }
        }
        private static List<byte> LeastConstraints(byte index, List<byte> values, byte[] assignment)
        {
            byte initialValue = assignment[index];
            List<byte[]> assignmentsList = [];
            List<byte> newValues = [];
            for (int i = 0; i < values.Count; i++)
            {
                assignment[index] = values[i];
                byte[] newAssignment = new byte[assignment.Length];
                assignment.CopyTo(newAssignment, 0);
                assignmentsList.Add(newAssignment);
            }
            assignment[index] = initialValue;

            CSPCompare cSPCompare = new();
            assignmentsList.Sort(cSPCompare);
            assignmentsList.Reverse();
            for (int i = 0; i < values.Count; i++)
            {
                newValues.Add(assignmentsList.First().ElementAt(index));
                assignmentsList.RemoveAt(0);
            }
            return newValues;
        }
        private static List<byte> values = [];
        private static byte[] OrderDomainValues(byte index, byte[] assignment)
        {
            values.Clear();
            for (int i = 0; i < boardSize; i++)
            {
                values.Add((byte)i);
            }


            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < values.Count; j++)
                    if (assignment[i] != 100 && assignment[i] == values[j])
                    {
                        values.RemoveAt(j);
                        j = -1;
                    }
            if(values.Count > 1)
                values = LeastConstraints(index, values, assignment);
            return [.. values];
        }
        private static byte[] heuristics = new byte[boardSize];
        private static void DegreeHeuristicsHandler()
        {
            for (byte i = 0; i < boardSize/2; i++)
                heuristics[i] = i;
            for (byte i = Convert.ToByte(boardSize / 2); i < boardSize; i++)
                heuristics[i] = (byte)(boardSize - i - 1);
            
        }
        private static byte SelectUnassignedVariable(byte[] assignment)
        {
            //using degree heuristic
            byte result = 0;
            for (byte i = 0; i < heuristics.Length; i++)
                if (assignment[i] == 100)
                    if (heuristics[result] <= heuristics[i])
                        result = i;
            return result;
        }
        private static string Inference(byte[] assignment)
        {
            byte nextIndex;
            bool isComplete = true;
            for (int i = 0; i < assignment.Length; i++)
                if (assignment[i] == 100)
                    isComplete = false;

            if (!isComplete)
            {
                nextIndex = SelectUnassignedVariable(assignment);
                if (OrderDomainValues(nextIndex, assignment).Length == 0 ||
                    OrderDomainValues(nextIndex, assignment).Length < boardSize - count)
                    return "failure";
            }
            return "success";
        }
        private static byte[] BacktrackingSearch(byte[] assignment)
        {
            bool isComplete = true;
            foreach (byte position in assignment)
                if(position == 100)
                    isComplete = false;
            if (isComplete)
                return assignment;
            
            byte index = SelectUnassignedVariable(assignment);
            foreach (byte value in OrderDomainValues(index, assignment))
            {
                if (IsConsistent(value, index, assignment))
                {
                    assignment[index] = value;
                    if (Inference(assignment) != "failure")
                    {
                        byte[] result = BacktrackingSearch(assignment);
                        isComplete = true;
                        foreach (byte position in assignment)
                            if (position == 100)
                            {
                                isComplete = false;
                                break;
                            }
                        if (isComplete)
                            if (GeneticFitnessCalculator(assignment) == maxConflicts)
                                return result;
                    }
                }
                assignment[index] = 100;
            }
            return [1];
        }
        private void CSPButton_Click(object sender, EventArgs e)
        {
            HideComponentsBeforeSolving(["actions", "time", "list"]);


            byte[] assignment = new byte[boardSize];
            for (int i = 0; i < boardSize; i++)
            {
                assignment[i] = 100;
            }

            Stopwatch sw = new();
            sw.Start();
            byte[] resultArray = BacktrackingSearch(assignment);
            sw.Stop();


            if (resultArray.Length == 1)
                MessageBox.Show("failure");
            else
                for (int i = 0; i < boardSize; i++)
                    for (int j = 0; j < boardSize; j++)
                    {
                        if (j == resultArray[i])
                            buttons.ElementAt(i)[j].Text = "♕";
                        else
                            buttons.ElementAt(i)[j].Text = "";
                    }

            DisplayComponentsAfterSolving(sw, ["time"]);

            WinCheck();
        }

        //===============================================================
        private void SizeTextBox_TextChanged(object sender, EventArgs e)
        {
            HideComponentsBeforeSolving(["actions", "time"]);

            if (sizeTextBox.Text != "")
            {
                boardSize = int.Parse(sizeTextBox.Text);

                heuristics = new byte[boardSize];
                DegreeHeuristicsHandler();

                maxConflicts = boardSize * (boardSize - 1) / 2;
                initialQueenPositionArray = new byte[boardSize];

                if (boardSize < 9 && boardSize > 3)
                    UpdateUI();
            }
        }
        private void NewButton_Click(object sender, EventArgs e)
        {
            HideComponentsBeforeSolving(["actions", "time"]);

            heuristics = new byte[boardSize];
            DegreeHeuristicsHandler();

            foreach (Button[] buttons in buttons)
                for (int i = 0; i < boardSize; i++)
                    buttons[i].Text = "";

            Random rn = new();
            for (int i = 0; i < boardSize; i++)
            {
                byte index = (byte)rn.Next(boardSize);
                initialQueenPositionArray[i] = index;
                Button btn = buttons.ElementAt(i)[index];
                btn.Text = "\u2655";
            }
        }
        private void CompareButton_Click(object sender, EventArgs e)
        {
            HideComponentsBeforeSolving(["actions", "time", "list"]);


            byte[] assignment = new byte[boardSize];
            for (int i = 0; i < boardSize; i++)
            {
                assignment[i] = 100;
            }
            Stopwatch sw1 = new();
            sw1.Start();
            byte[] resultArray2 = BacktrackingSearch(assignment);
            sw1.Stop();

            string CSPText;
            if (resultArray2.Length == 1)
                CSPText = "failure";
            else
                CSPText = sw1.ElapsedMilliseconds.ToString();


            List<byte[]> initialPopulation = [];
            for (int i = 0; i < initialPopulationCount; i++)
            {
                NewButton.PerformClick();
                byte[] tempQueenPositionArray = new byte[boardSize];
                for (int j = 0; j < boardSize; j++)
                {
                    tempQueenPositionArray[j] = initialQueenPositionArray[j];
                }
                initialPopulation.Add(tempQueenPositionArray);
            }
            Stopwatch sw = new();
            sw.Start();
            byte[] resultArray = Genetic(initialPopulation);
            sw.Stop();

            

            HideComponentsBeforeSolving(["actions", "clear-list", "time", "list"]);

            Stopwatch sw2 = new();
            sw2.Start();
            string hillClimbingResult = HillClimb();
            sw2.Stop();

           
            string hillClimbingText;
            if (hillClimbingResult == "failure")
            {
                hillClimbingText = "failure";
                DisplayComponentsAfterSolving(sw, ["time"]);
            }
            else
            {
                DisplayComponentsAfterSolving(sw2, ["time", "actions", "list"]);
                hillClimbingText = sw2.ElapsedMilliseconds.ToString();
            }
            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++)
                {
                    if (j == resultArray[i])
                        buttons.ElementAt(i)[j].Text = "♕";
                    else
                        buttons.ElementAt(i)[j].Text = "";
                }

            WinCheck();

            MessageBox.Show($"CSP Time: {CSPText}\nGenetic Time: {sw.ElapsedMilliseconds}\nHill Climbing Time: {hillClimbingText}");
        }
        
    }
}
