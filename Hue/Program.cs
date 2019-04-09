using System;
using System.Collections.Generic;

namespace Hue {
    namespace K_Means_NS {
        class K_Means {
            static void K_Means_Main(string[] args) {
                Console.WriteLine($@"\nBegin k-means clustering demo\n");
                #region setup
                double[][] rawData = new double[10][];
                rawData[0] = new double[] { 73, 72.6 };
                rawData[1] = new double[] { 61, 54.4 };
                rawData[2] = new double[] { 67, 99.9 };
                rawData[3] = new double[] { 68, 97.3 };
                rawData[4] = new double[] { 62, 59.0 };
                rawData[5] = new double[] { 75, 81.6 };
                rawData[6] = new double[] { 74, 77.1 };
                rawData[7] = new double[] { 66, 97.3 };
                rawData[8] = new double[] { 68, 93.3 };
                rawData[9] = new double[] { 61, 59.0 };

                //double[][] rawData = LoadData("..\\..\\HeightWeight.txt", 10, 2, ',');
                #endregion
                Console.WriteLine("Raw unclustered height (in.) weight (kg.) data:\n");
                Console.WriteLine(" ID   Height   Weight");
                Console.WriteLine("----------------------");
                ShowData(rawData, 1, true, false);

                int numClusters = 3;
                Console.WriteLine("\nSetting numClusters to " + numClusters);

                Console.WriteLine("Starting clustering using k-means algorithm");
                Clusterer c = new Clusterer(numClusters);
                int[] clustering = c.Cluster(rawData);
                Console.WriteLine("Clustering complete\n");

                Console.WriteLine("Final clustering in internal form:\n");
                ShowVector(clustering, true);

                Console.WriteLine("Raw data by cluster:\n");
                ShowClustered(rawData, clustering, numClusters, 1);

                Console.WriteLine("\nEnd k-means clustering demo\n");
                Console.ReadLine();
            }

            static void ShowData(double[][] data, int decimals, bool indices, bool newLine) {
                for (int i = 0; i < data.Length; i++) {
                    if (indices)
                        Console.WriteLine(i.ToString().PadLeft(3) + "  ");
                    for (int j = 0; j < data[i].Length; j++) {
                        double v = data[i][j];
                        Console.Write(v.ToString("F" + decimals) + "    ");
                        Console.Write("");
                    }
                    if (newLine)
                        Console.WriteLine("");
                }
            }
            static void ShowVector(int[] vector, bool newLine) {
                for (int i = 0; i < vector.Length; i++) {
                    Console.Write(vector[i] + " ");
                }
                if (newLine)
                    Console.Write("\n");
            }
            static void ShowClustered(double[][] data, int[] clustering, int numClusters, int decimals) {
                for (int k = 0; k < numClusters; k++) {
                    Console.WriteLine("====================");
                    for (int i = 0; i < data.Length; i++) {
                        int clusterID = clustering[i];
                        if (clusterID != k)
                            continue;
                        Console.Write(i.ToString().PadLeft(3) + "  ");
                        for (int j = 0; j < data[i].Length; j++) {
                            double v = data[i][j];
                            Console.Write(v.ToString("F" + decimals) + "    ");
                        }
                        Console.WriteLine("");
                    }
                }
                Console.WriteLine("====================");
            }

            public class Clusterer {
                private int numClusters;      // Number of clusters
                private int[] clustering;     // Index = a tuple, value = clusterID
                private double[][] centroids; // Mean (vector) of each cluster
                private Random rnd;           // For initialisation

                public Clusterer(int numClusters) {
                    this.numClusters = numClusters;
                    this.centroids = new double[numClusters][];
                    this.rnd = new Random(0); //arbitrary seed
                }

                public int[] Cluster(double[][] data) {
                    int numTuples = data.Length;         //# number of data points
                    int numValues = data[0].Length;      //# width of a data point
                    this.clustering = new int[numTuples]; //# size = source data, domain = numClusters (3)
                    for (int k = 0; k < numClusters; k++)  //allocate each centroid
                        this.centroids[k] = new double[numValues];
                    InitRandom(data);

                    Console.WriteLine("\nInitial random clustering:");
                    for (int i = 0; i < clustering.Length; i++) {
                        Console.Write(clustering[i] + " ");
                    }
                    Console.WriteLine("\n");

                    bool changed = true; //Change in clustering?
                    int maxCount = numTuples * 10; // sanity check
                    int ct = 0;
                    while (changed == true && ct <= maxCount) {
                        ++ct; // k-means typically converges very quickly
                        UpdateCentroids(data); //No effect if fail
                        changed = UpdateClustering(data);//No effect if fail
                    }

                    int[] result = new int[numTuples];
                    Array.Copy(this.clustering, result, clustering.Length);
                    return result;

                }
                private void InitRandom(double[][] data) {
                    int numTuples = data.Length; //# number of data points

                    int clusterID = 0;
                    for (int i = 0; i < numTuples; i++) {
                        clustering[i] = clusterID++;
                        if (clusterID == numClusters)
                            clusterID = 0;
                    }
                    //for (int i = 0; i < numTuples; i++) {
                    //    int r = rnd.Next(i, clustering.Length);
                    //    int tmp = clustering[r];
                    //    clustering[r] = clustering[i];
                    //    clustering[i] = tmp;
                    //}
                }
                private void UpdateCentroids(double[][] data) {
                    int[] clusterCounts = new int[numClusters];
                    for (int i = 0; i < data.Length; i++) {
                        int clusterID = clustering[i];
                        ++clusterCounts[clusterID];
                    }

                    // zero-out this.centroids so it can be used as scratch
                    for (int k = 0; k < centroids.Length; k++)
                        for (int j = 0; j < centroids[k].Length; j++)
                            centroids[k][j] = 0.0;
                    for (int i = 0; i < data.Length; i++) {
                        int clusterID = clustering[i];
                        for (int j = 0; j < data[i].Length; j++)
                            centroids[clusterID][j] += data[i][j]; //accumulate sum                        
                    }
                    for (int k = 0; k < centroids.Length; k++)
                        for (int j = 0; j < centroids[k].Length; j++)
                            centroids[k][j] /= clusterCounts[k]; // danger?
                }
                private bool UpdateClustering(double[][] data) {
                    // (re)assign each tuple to a cluster (closest centroid)
                    // returns false if no tuple assignments change OR
                    // if the reassignment would result in a clustering where
                    // one or more clusters have no tuples

                    bool changed = false; // did any tuple change cluster?

                    int[] newClustering = new int[clustering.Length]; // proposed result
                    Array.Copy(clustering, newClustering, clustering.Length);

                    double[] distances = new double[numClusters]; // from tuple to centroids

                    for (int i = 0; i < data.Length; i++) {
                        for (int k = 0; k < numClusters; k++)
                            distances[k] = Distance(data[i], centroids[k]);

                        int newClusterID = MinIndex(distances); // find closest centroid
                        if (newClusterID != newClustering[i]) {
                            changed = true; // note a new clustering
                            newClustering[i] = newClusterID; //accept update
                        }
                    }

                    if (!changed)
                        return false; // no change so bail

                    // check proposed clustering cluster counts
                    int[] clusterCounts = new int[numClusters];
                    for (int i = 0; i < data.Length; i++) {
                        int clusterID = newClustering[i];
                        ++clusterCounts[clusterID];
                    }

                    for (int k = 0; k < numClusters; k++)
                        if (clusterCounts[k] == 0)
                            return false;//bad clustering

                    // alternative: place a random data item into empty cluster
                    //for (int k = 0; k < numClusters; k++) {
                    //    if(clusterCounts[k] == 0) { //cluster k has no items
                    //        for (int t = 0; t < data.Length; t++) {// find a tuple to put into cluster k
                    //            int cid = newClustering[t];  // cluster of t
                    //            int ct = clusterCounts[cid]; // how many more items?
                    //            if(ct >= 2) {// t is in a cluster w/ 2 or more items
                    //                newClustering[t] = k; //place t into cluster k
                    //                ++clusterCounts[k];   //k now has a data item
                    //                --clusterCounts[cid]; //cluster that used to have t
                    //                break;                //check next cluster
                    //            }
                    //        }// t
                    //    } // cluster count of 0
                    //}  // k
                    Array.Copy(newClustering, clustering, newClustering.Length); // update
                    return true; // good clustering and at least one change
                } //Update clustering
                private static double Distance(double[] tuple, double[] centroid) {
                    //2D, plottable on a graph...

                    // Euclidean distance between two vectors for UpdateClustering()
                    double sumSquaredDiffs = 0.0;
                    for (int j = 0; j < tuple.Length; j++)
                        sumSquaredDiffs += (tuple[j] - centroid[j]) * (tuple[j] - centroid[j]);
                    return Math.Sqrt(sumSquaredDiffs);
                }
                private static int MinIndex(double[] distances) {
                    // helper for UpdateClustering()  to find closest centroid
                    int indexOfMin = 0;
                    double smallDist = distances[0];
                    for (int k = 0; k < distances.Length; k++) {
                        if (distances[k] < smallDist) {
                            smallDist = distances[k];
                            indexOfMin = k;
                        }
                    }
                    return indexOfMin;
                }
            }//Clusterer
        }//ns
    }
    namespace GAGUC_NS {
        class GACUK {

            static void Main(string[] args) {
                var gacuk = new notStatic();
                gacuk.Go();
            }
            public class notStatic {
                public void Go() {
                    Console.WriteLine("\nBegin categorical data clustering demo\n");

                    string[][] rawData = new string[7][];

                    rawData[0] = new string[] { "Blue", "Small", "False" };
                    rawData[1] = new string[] { "Green", "Medium", "True" };
                    rawData[2] = new string[] { "Red", "Large", "False" };
                    rawData[3] = new string[] { "Red", "Small", "True" };
                    rawData[4] = new string[] { "Green", "Medium", "False" };
                    rawData[5] = new string[] { "Yellow", "Medium", "False" };
                    rawData[6] = new string[] { "Red", "Large", "False" };

                    Console.WriteLine("Raw unclustered data:\n");
                    Console.WriteLine("    Color    Size    Heavy");
                    Console.WriteLine("----------------------------");
                    ShowData(rawData);

                    int numClusters = 2;
                    Console.WriteLine("\nSetting numClusters to " + numClusters);
                    int numRestarts = 4; //Sqrt(Data.Length)
                    Console.WriteLine("Setting numRestarts to " + numRestarts);

                    Console.WriteLine("\nStarting clustering using greedy category utility");
                    CatClusterer cc = new CatClusterer(numClusters, rawData); //restart version
                    double cu;
                    int[] clustering = cc.Cluster(numRestarts, out cu);
                    Console.WriteLine("Clustering complete.\n");

                    Console.WriteLine("Final clustering in internal form:");
                    ShowVector(clustering, true);

                    Console.WriteLine("Final CU value = " + cu.ToString("F4"));

                    Console.WriteLine("\nRaw data grouped by cluster:\n");
                    ShowClustering(numClusters, clustering, rawData);

                    Console.WriteLine("\nEnd categorical data clustering demo\n");
                    Console.ReadLine();
                }

                public void ShowData(string[][] matrix) { // for tuples
                    for (int i = 0; i < matrix.Length; i++) {
                        Console.Write("[" + i + "] ");
                        for (int j = 0; j < matrix[i].Length; j++)
                            Console.Write(matrix[i][j].ToString().PadRight(8) + " ");
                        Console.WriteLine("");
                    }
                }

                public void ShowVector(int[] vector, bool newLine) { // for clustering
                    for (int i = 0; i < vector.Length; i++)
                        Console.Write(vector[i] + " ");
                    Console.WriteLine("");
                    if(newLine)
                        Console.WriteLine("");
                }

                public void ShowClustering(int numClusters, int[] clustering, string[][] rawData) {
                    Console.WriteLine("--------------------------------------");
                    for (int k = 0; k < numClusters; k++) { // display by cluster
                        for (int i = 0; i < rawData.Length; i++) { // each tuple
                            if(clustering[i] == k) { // curr tuple i belongs to curr cluster k
                                Console.Write(i.ToString().PadLeft(2) + "  ");
                                for (int j = 0; j < rawData[i].Length; j++) {
                                    Console.Write(rawData[i][j].ToString().PadRight(8) + " ");
                                }
                                Console.WriteLine("");
                            }
                        }
                        Console.WriteLine("--------------------------------------");
                    }
                }

                public class CatClusterer {
                    private int numClusters;        // number of clusters
                    private int[] clustering;       // index = a tuple, value = cluster ID
                    private int[][] dataAsInts;     // ex: red = 0, blue = 1, green = 2
                    private int[][][] valueCounts;  // scratch to compute CU [att][va][count]
                    private int[] clusterCounts;    // number tuples assigned to each cluster (sum)
                    private Random rnd;

                    public CatClusterer(int numClusters, string[][] rawData) {
                        this.numClusters = numClusters;
                        MakeDataMatrix(rawData); // convert strings to ints into this.dataAsInts[][]
                        Allocate(); // allocate all arrays and matrices (no initialise values)
                    }

                    public int[] Cluster(int numRestarts, out double catUtility) {
                        // restart version
                        int numRows = dataAsInts.Length;
                        double currCU, bestCU = 0.0;
                        int[] bestClustering = new int[numRows];
                        for (int start = 0; start < numRestarts; start++) {
                            int seed = start; // use the start index as rnd seed
                            int[] currClustering = ClusterOnce(seed, out currCU);
                            if(currCU > bestCU) {
                                bestCU = currCU;
                                Array.Copy(currClustering, bestClustering, numRows);
                            }
                        }
                        catUtility = bestCU;
                        return bestClustering;
                    } // Cluster

                    private int[] ClusterOnce(int seed, out double catUtility) {
                        this.rnd = new Random();
                        Initialise(); //clustering[] to -1, all counts[] to 0;

                        int numTrials = dataAsInts.Length;             // for initial tuple assignments
                        int[] goodIndexes = GetGoodIndices(numTrials); // tuples that are dissimilar
                        for (int k = 0; k < numClusters; k++)          // assign first tuples to clusters
                            Assign(goodIndexes[k], k);

                        int numRows = dataAsInts.Length;
                        int[] rndSequence = new int[numRows];
                        for (int i = 0; i < numRows; i++)
                            rndSequence[i] = i;
                        Shuffle(rndSequence); // present tuples in random sequence

                        for (int t = 0; t < numRows; t++) { // main loop. walk through each tuple
                            int idx = rndSequence[t]; // index of data tuple to process
                            if (clustering[idx] != -1) continue; // tuple clustered by initialisation

                            double[] candidateCU = new double[numClusters]; // candidate CU values

                            for (int k = 0; k < numClusters; k++) {
                                Assign(idx, k); // tentative cluster assignment
                                candidateCU[k] = CategoryUtility(); // compute and save the CU
                                Unassign(idx, k); // undo tentative assignment
                            }

                            int bestK = MaxIndex(candidateCU); // greedy. the index is a clusterID
                            Assign(idx, bestK); // now we know which cluster gave the best CU
                        } // each tuple

                        catUtility = CategoryUtility();
                        int[] result = new int[numRows];
                        Array.Copy(this.clustering, result, numRows);
                        return result;
                    } // Clustering

                    private void MakeDataMatrix(string[][] rawData) {
                        int numRows = rawData.Length;
                        int numCols = rawData[0].Length;

                        this.dataAsInts = new int[numRows][]; // allocate all
                        for (int i = 0; i < numRows; i++)
                            dataAsInts[i] = new int[numCols];

                        for (int col = 0; col < numCols; col++) {
                            int idx = 0;
                            Dictionary<string, int> dict = new Dictionary<string, int>();
                            for (int row = 0; row < numRows; row++) { // build dict for curr col
                                string s = rawData[row][col];
                                if (dict.ContainsKey(s) == false)
                                    dict.Add(s, idx++);
                            }
                            for (int row = 0; row < numRows; row++) {
                                string s = rawData[row][col];
                                int v = dict[s];
                                this.dataAsInts[row][col] = v;
                            }
                        }
                        return; //explicit return style
                    }

                    private void Allocate() {
                        // assumes dataAsInts has been created
                        // allocate this.clustering[], this.clusterCounts[], this.valueCounts[][][]

                        int numRows = dataAsInts.Length;
                        int numCols = dataAsInts[0].Length;

                        this.clustering = new int[numRows];
                        this.clusterCounts = new int[numClusters + 1]; // last cell is sum

                        this.valueCounts = new int[numCols][][]; // 1st dim

                        for (int col = 0; col < numCols; col++) { // need # distinct values in each col
                            int maxVal = 0;
                            for (int i = 0; i < numRows; i++) {
                                if (dataAsInts[i][col] > maxVal)
                                    maxVal = dataAsInts[i][col];
                            }
                            this.valueCounts[col] = new int[maxVal + 1][]; // 0-based 2nd dim
                        }

                        for (int i = 0; i < this.valueCounts.Length; i++)
                            for (int j = 0; j < this.valueCounts[i].Length; j++)
                                this.valueCounts[i][j] = new int[numClusters + 1]; // +1 last cell is sum

                        return;
                    }

                    private void Initialise() {
                        for (int i = 0; i < clustering.Length; i++) 
                            clustering[i] = -1;
                        for (int i = 0; i < clusterCounts.Length; i++)
                            clusterCounts[i] = 0;
                        for (int i = 0; i < valueCounts.Length; i++)
                            for (int j = 0; j < valueCounts[i].Length; j++)
                                for (int k = 0; k < valueCounts[i][j].Length; k++)
                                    valueCounts[i][j][k] = 0;
                        return;
                    }

                    private double CategoryUtility() { // called by clusterOnce
                        // because CU is called many times use precomputed counts
                        int numTuplesAssigned = clusterCounts[clusterCounts.Length - 1]; // last cell

                        double[] clusterProbs = new double[this.numClusters];
                        for (int k = 0; k < numClusters; k++)
                            clusterProbs[k] = (clusterCounts[k] * 1.0) / numTuplesAssigned;

                        // single unconditional prob term
                        double unconditional = 0.0;
                        for (int i = 0; i < valueCounts.Length; i++) {
                            for (int j = 0; j < valueCounts[i].Length; j++) {
                                int sum = valueCounts[i][j][numClusters]; // last cell holds sum
                                double p = (sum * 1.0) / numTuplesAssigned;
                                unconditional += (p * p);
                            }
                        }

                        // conditional terms each cluster
                        double[] conditionals = new double[numClusters];
                        for (int k = 0; k < numClusters; k++) {
                            for (int i = 0; i < valueCounts.Length; i++) { // each att
                                for (int j = 0; j < valueCounts[i].Length; j++) { // each value
                                    double p = (valueCounts[i][j][k] * 1.0) / clusterCounts[k];
                                    conditionals[k] += (p * p);
                                }
                            }
                        }

                        // we have P(Ck), EE P(Ai=Vij|Ck)^2, EE P(Ai=Vij)^2 so we can compute CU easily
                        double summation = 0.0;
                        for (int k = 0; k < numClusters; k++)
                            summation += clusterProbs[k] * (conditionals[k] - unconditional);
                        //E P(Ck) * [EE P(Ai=Vij|Ck)^2 - EE P(Ai=Vij)^2] / n

                        return summation / numClusters;                        
                    }  //CategoryUtility

                    private int MaxIndex(double[] cus) {
                        // Helper for ClusterOnce. returns index of largest value in array
                        double bestCU = 0.0;
                        int indexOfBestCU = 0;
                        for (int k = 0; k < cus.Length; k++) {
                            if(cus[k] > bestCU) {
                                bestCU=cus[k];
                                indexOfBestCU = k;
                            }
                        }
                        return indexOfBestCU;
                    }

                    private void Shuffle(int[] indices) { // instance so can use class rnd
                        for (int i = 0; i < indices.Length; i++) { // Fisher-Yates shuffle
                            int ri = rnd.Next(i, indices.Length); // random index
                            int tmp = indices[i];
                            indices[i] = indices[ri]; // swap
                            indices[ri] = tmp;
                        }
                    }

                    private void Assign(int dataIndex, int clusterID) {
                        // assign tuple at dataIndex to clustering[] cluster, and
                        // update valueCounts[][][], clusterCounts[]
                        clustering[dataIndex] = clusterID; // assign

                        for (int i = 0; i < valueCounts.Length; i++) { // update valueCounts
                            int v = dataAsInts[dataIndex][i]; // att value
                            ++valueCounts[i][v][clusterID];   // bump count
                            ++valueCounts[i][v][numClusters]; // bump sum
                        }
                        ++clusterCounts[clusterID];   // update clusterCounts
                        ++clusterCounts[numClusters]; // last cell is sum
                    }

                    private void Unassign(int dataIndex, int clusterID) {
                        clustering[dataIndex] = -1; // unassign
                        for (int i = 0; i < valueCounts.Length; i++) { // update
                            int v = dataAsInts[dataIndex][i];
                            --valueCounts[i][v][clusterID];
                            --valueCounts[i][v][numClusters]; // last cell is sum
                        }
                        --clusterCounts[clusterID];   // update clusterCounts
                        --clusterCounts[numClusters]; // last cell
                    }

                    private int[] GetGoodIndices(int numTrials) {
                        // return numClusters indices of tuples that are different
                        int numRows = dataAsInts.Length;
                        int numCols = dataAsInts[0].Length;
                        int[] result = new int[numClusters];

                        int largestDiff = -1; // Difference for a set of numClusters tuples
                        for (int trials = 0; trials < numTrials; trials++) {
                            int[] candidates = Reservoir(numClusters, numRows);
                            int numDifferences = 0; // for these candidates
                            for (int i = 0; i < candidates.Length; i++) { // all possible pairs
                                for (int j = 0; j < candidates.Length; j++) {
                                    int aRow = candidates[i];
                                    int bRow = candidates[j];

                                    for (int col = 0; col < numCols; col++)
                                        if (dataAsInts[aRow][col] != dataAsInts[bRow][col])
                                            ++numDifferences;
                                }
                            }
                            #region comment
                            //for (int i = 0; i < candidates.Length; i++) { // only adjacent pairs
                            //    int aRow = candidates[i];
                            //    int bRow = candidates[i + 1];
                            //    for (int col = 0; col < numCols; col++)
                            //        if (dataAsInts[aRow][col] != dataAsInts[bRow][col])
                            //            ++numDifferences;
                            //}
                            #endregion
                            if(numDifferences > largestDiff) {
                                largestDiff = numDifferences;
                                Array.Copy(candidates, result, numClusters);
                            }
                        } // trial
                        return result;
                    }

                    private int[] Reservoir(int n, int range) { // helper for GetGoodIndices
                        // select n random indices between [0, range)
                        int[] result = new int[n];
                        for (int i = 0; i < n; i++)
                            result[i] = i;

                        for (int t = n; t < range; t++) {
                            int j = rnd.Next(0, t + 1);
                            if (j < n)
                                result[j] = t;
                        }
                        return result;
                    }
                } // CatClusterer
            }
        }
    }
}

