using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data;

namespace CpuScheduling
{
    class Algo
    {
        TextBox output;
        List<Process> Grant_Chart = new List<Process>();
        My_CPU cpu = new My_CPU();

        
        public Algo(TextBox tb)
        {
            this.output = tb;
            
        }

        public static string[] SpritString(String line)
        {
            char[] delimeter = { ' ', '\t' };
            string[] word = line.Split(delimeter, StringSplitOptions.RemoveEmptyEntries);
            return word;

        }

        public bool StillProcessToWork(List<Process> processList)
        {
            int n = processList.Count;
            for (int i = 0; i < n; i++)
            {
                if (!processList[i].IsFinish)
                {
                    return true;
                }

            }
            return false;

        }

       

       
        
        void swapProcesses(int i, int i2, List<Process> processList)
        {
            Process temp = processList[i];
            processList[i] = processList[i2];
            processList[i2] = temp;
        }


        public void SortProcess(String by, List<Process> processes)
        {
            int j, i;
            int n = processes.Count;
            switch (by)
            {
                case "byArriveTime":
                    for (j = 0; j < n; j++)
                        for (i = 0; i <n - 1; i++)
                            if (processes[i].arriveTime > processes[i + 1].arriveTime)
                                swapProcesses(i, i + 1, processes);
                      break;


                case "byBurstTime":
                    for (j = 0; j < n; j++)
                        for (i = 0; i < n - 1; i++)
                        {
                            if (processes[i].burstTime == processes[i + 1].burstTime)
                                if (processes[i].arriveTime > processes[i + 1].arriveTime)
                                    swapProcesses(i, i + 1, processes);
                            if (processes[i].burstTime > processes[i + 1].burstTime)
                                swapProcesses(i, i + 1,processes);
                        }
               
                    break;

                case "byRemainingTime":
                    for (j = 0; j < n; j++)
                        for (i = 0; i < n - 1; i++)
                        {
                            if (processes[i].time_remaining == processes[i + 1].time_remaining)
                                if (processes[i].arriveTime > processes[i + 1].arriveTime)
                                    swapProcesses(i, i + 1,processes);
                            if (processes[i].time_remaining > processes[i + 1].time_remaining)
                                swapProcesses(i, i + 1,processes);
                        }
                    
                    break;

                case "byPriority":
                    for ( j = 0; j < n; j++)
                        for ( i = 0; i < n - 1; i++)
                        {
                            if (processes[i].priority == processes[i + 1].priority)
                                if (processes[i].arriveTime > processes[i + 1].arriveTime)
                                    swapProcesses(i, i + 1,processes);
                            if (processes[i].priority > processes[i + 1].priority)
                                swapProcesses(i, i + 1,processes);
                        }
                   
                    break;
                
                case "byName":
                    for (j = 0; j < n; j++)
                        for (i = 0; i < n - 1; i++)
                            if (processes[i].getName() > processes[i + 1].getName())
                                swapProcesses(i, i + 1,processes);
                    break;
                  
            }
     
            
        }

        int getProcessIndexByName(String name,List<Process> processList)
        {
            int n = processList.Count;
            int index = -1;
            for (int i = 0; i < n; i++)
                if (processList[i].name == name)
                {
                    index = i;
                    break;
                }
            return index;
        }

        public void preemtivePriority(List<Process> processList)
        {
            Grant_Chart.Clear();
            int time, currentIndex, bigValue = 100000;
            int n = processList.Count;
           
            SortProcess("byPriority",processList);
            cpu.priority = bigValue;
            for (time = 0; StillProcessToWork(processList); time++)
            {
                
                SortProcess("byPriority", processList);
                for (int i = 0; i < n; i++)
                {
                  
                    if (!processList[i].IsFinish && processList[i].arriveTime <= time && processList[i].priority <= cpu.priority)
                    {
                       
                        if (!cpu.IsIdle)
                        {
                            if (i == getProcessIndexByName(cpu.Inside_Process.name, processList))
                            {
                                break;
                            }
                        }

                        if (processList[i].start_time == -1)
                            processList[i].start_time = time;
                   
                        processList[i].putIntimes.Add(time);
                        Grant_Chart.Add(processList[i]);
                        cpu.Inside_Process = processList[i];
                        cpu.IsIdle = false;
                       cpu.priority = processList[i].priority;
                    
                        break;
                    }
                }
                if (!cpu.IsIdle)
                {                 
                    currentIndex = getProcessIndexByName(cpu.Inside_Process.name,processList);                    
                }                      
                else
                    continue;
             
                processList[currentIndex].decrementTimeRemaining(1);
                
                if (processList[currentIndex].time_remaining == 0)
                {
                  
                    processList[currentIndex].finish_time = time+1;
                    processList[currentIndex].IsFinish = true;
                    processList[currentIndex].waiting_time =processList[currentIndex].finish_time - processList[currentIndex].burstTime - processList[currentIndex].arriveTime;
                    processList[currentIndex].CalculateTurnAroundTime();
                    cpu.priority = bigValue;
                    cpu.IsIdle = true;
                }
             
                
            }
            SortProcess("byName", processList);
            Conclusion(processList);
        }

        int GetTotalCPUTime(List<Process> processList)
        {
            int TotalProcessingTime = 0;
            for (int i = 0; i < processList.Count; i++)
                TotalProcessingTime += processList[i].burstTime;

            return TotalProcessingTime;
        }

        public void RR(List<Process> processList, int quantum)
        {
            Grant_Chart.Clear();
            int time = 0;
            cpu.IsIdle = true;
            SortProcess("byArriveTime", processList);
          
            int  q = 0;
        
            while (StillProcessToWork(processList))
            {
                for (int i = 0; i < processList.Count; i++)
                {
                    if (q == 0 && !processList[i].IsFinish && cpu.IsIdle && processList[i].arriveTime <= time)
                    {
                        if (processList[i].start_time == -1)
                            processList[i].start_time = time;
                        processList[i].putIntimes.Add(time);
                        Grant_Chart.Add(processList[i]);
                      
                        
                        cpu.IsIdle = false;
                       cpu.Inside_Process = processList[i];
                    }
                    if (cpu.IsIdle)
                    {
                       
                        continue;
                    }

                    for (; StillProcessToWork(processList);time++ )
                    {
                        processList[getProcessIndexByName(cpu.Inside_Process.name,processList)].decrementTimeRemaining(1);
                     
                        if (processList[getProcessIndexByName(cpu.Inside_Process.name,processList)].time_remaining == 0)
                        {
                            time++;
                            q = 0;
                            cpu.IsIdle = true;
                            int currentIndex = getProcessIndexByName(cpu.Inside_Process.name,processList);
                            processList[currentIndex].finish_time = time;
                            processList[currentIndex].waiting_time = processList[currentIndex].finish_time - processList[currentIndex].burstTime - processList[currentIndex].arriveTime;
                            processList[currentIndex].CalculateTurnAroundTime();
                            processList[currentIndex].IsFinish = true;
                        
                            
                            break;
                        }
                        q++;
                        if (q == quantum)
                        {
                            q = 0;
                           cpu.IsIdle = true;
                            time++;
                            break;
                        }
                      
                    }
                }
            }
            
            SortProcess("byName", processList);
            Conclusion(processList);
            
        }

        void RollBackTimeRemaining(List<Process> ProcessList)
        {
            for (int i = 0; i < ProcessList.Count; i++)
            {
                ProcessList[i].time_remaining = ProcessList[i].burstTime;

            }

        }


       public void showRR()
        {
            int i= 3;
            Process p = Grant_Chart[i];
            foreach (int a in Grant_Chart[i].putIntimes)
            {
                Console.WriteLine(a);
            }
        }

       public void Conclusion(List<Process> processList)
       {
           output.Text = "Process Name" + "\t" + "Waiting Time" + "\t" + "TurnAround Time";
           output.AppendText("\n");
           int totalWait = 0;
           int totalTurn = 0;
           foreach (Process p in processList)
           {
               totalWait += p.waiting_time;
               totalTurn+=p.turnaround_time;
               output.AppendText(p.name + "\t\t    " + p.waiting_time + "\t\t    " + p.turnaround_time);
               output.AppendText("\n");
           }
           double n = processList.Count;
           double averageWait = totalWait / n;
           double aveageTurn = totalTurn / n;
           output.AppendText("Ave WT" + averageWait+"\n");
           output.AppendText("Ave TT= " + aveageTurn+"\n");  
       }

        public DataTable showGrantChart()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Process", typeof(String));
            table.Columns.Add("In", typeof(int));
            table.Columns.Add("Out", typeof(int));
          
            RollBackTimeRemaining(Grant_Chart);
           
            for (int i = 0; i < Grant_Chart.Count; i++)
            {
                int count = Grant_Chart[i].countIntime;
                

                if (i < Grant_Chart.Count - 1)
                {
                    int count2 = Grant_Chart[i + 1].countIntime;

                    int burstTimeNow = Grant_Chart[i + 1].putIntimes[count2] - Grant_Chart[i].putIntimes[count];
                    int timeRemaining = Grant_Chart[i].time_remaining - burstTimeNow;
                    

                    if (timeRemaining >= 0)
                    {
                        if (Grant_Chart[i].name != Grant_Chart[i + 1].name)
                        {
                            Console.WriteLine(Grant_Chart[i].name + " " + Grant_Chart[i].putIntimes[count] + " " + Grant_Chart[i + 1].putIntimes[count2] + " " + count2);
                            Grant_Chart[i].time_remaining = timeRemaining;
                            table.Rows.Add(Grant_Chart[i].name, Grant_Chart[i].putIntimes[count], Grant_Chart[i + 1].putIntimes[count2]);
                        }
                        else
                        {
                            Console.WriteLine(Grant_Chart[i].name + " " + Grant_Chart[i].putIntimes[count] + " " + Grant_Chart[i + 1].putIntimes[count+1] );
                            table.Rows.Add(Grant_Chart[i].name, Grant_Chart[i].putIntimes[count], Grant_Chart[i + 1].putIntimes[count+1]);
                        }
                    }
                    else
                    {
                        table.Rows.Add(Grant_Chart[i].name, Grant_Chart[i].putIntimes[count], (Grant_Chart[i].putIntimes[count] + Grant_Chart[i].time_remaining));
                        Grant_Chart[i].time_remaining = 0;
                    }
                }
                else
                {
                    table.Rows.Add(Grant_Chart[i].name, Grant_Chart[i].putIntimes[count], Grant_Chart[i].finish_time);
                }
                Grant_Chart[i].countIntime++;
            }
            return table;
        }

        
    }
}
