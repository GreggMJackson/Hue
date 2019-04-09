using System;
using System.Collections.Generic;

namespace Natalie {
    class CodeWithMain {
        static void Main(string[] args) {
            var classNotStatic = new ClassNotStatic();
            classNotStatic.Go();
        }
    
        static void ShowData(List<myClusterable> data, int decimals, bool indices, bool newLine) {
            for (int i = 0; i < data.Count; i++) {
                if (indices)
                    Console.WriteLine(i.ToString().PadLeft(3) + "  ");
                for (int j = 0; j < data[i]._item.FieldCount(); j++) {
                    double v = data[i]._item.FieldFuncs[j].Quantify();
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
        static void ShowClustered(List<myClusterable> data, int[] clustering, int numClusters, int decimals) {
            for (int k = 0; k < numClusters; k++) {
                Console.WriteLine("====================");
                for (int i = 0; i < data.Count; i++) {
                    int clusterID = clustering[i];
                    if (clusterID != k)
                        continue;
                    Console.Write(i.ToString().PadLeft(3) + "  ");
                    for (int j = 0; j < data[i]._item.FieldCount(); j++) {
                        double v = data[i]._item.FieldFuncs[j].Quantify();
                        Console.Write(v.ToString("F" + decimals) + "    ");
                    }
                    Console.WriteLine("");
                }
            }
            Console.WriteLine("====================");
        }

        public class ClassNotStatic {
            public void Go() {
                var slow_VI = new List<myClusterable>();
                slow_VI = GetData();
                Console.WriteLine("Raw unclustered data:\n");
                Console.WriteLine("----------------------");
                ShowData(slow_VI, 1, true, false);
                var c = new Clusterer();
                c.Data = slow_VI;

                Console.WriteLine("\nSetting numClusters to 3");
                Console.WriteLine("Starting clustering using k-means algorithm");

                int numClusters = c.numCentroids = 3;
                int[] results = c.Cluster();

                Console.WriteLine("Clustering complete\n");
                Console.WriteLine("Final clustering in internal form:\n");

                ShowVector(results, true);

                Console.WriteLine("Raw data by cluster:\n");
                ShowClustered(slow_VI, results, numClusters, 1);

                Console.WriteLine("OK, done");
                //Console.ReadLine();
            }
            private List<myClusterable> GetData() {
                return new List<myClusterable>() {
                    new myClusterable(new gbcOneWrap( new gonnaBeClusteredOne(73,"72.6",new Object()))),
                    new myClusterable(new gbcOneWrap( new gonnaBeClusteredOne(61,"54.4",new Object()))),
                    new myClusterable(new gbcOneWrap( new gonnaBeClusteredOne(67,"99.9",new Object()))),
                    new myClusterable(new gbcOneWrap( new gonnaBeClusteredOne(68,"97.3",new Object()))),
                    new myClusterable(new gbcOneWrap( new gonnaBeClusteredOne(62,"59.0",new Object()))),
                    new myClusterable(new gbcTooWrap( new gonnaBeClusteredToo("75",81.6,new Object()))),
                    new myClusterable(new gbcTooWrap( new gonnaBeClusteredToo("74",77.1,new Object()))),
                    new myClusterable(new gbcTooWrap( new gonnaBeClusteredToo("66",97.3,new Object()))),
                    new myClusterable(new gbcTooWrap( new gonnaBeClusteredToo("68",93.3,new Object()))),
                    new myClusterable(new gbcTooWrap( new gonnaBeClusteredToo("61",59.0,new Object()))),
                };
            }
        }
        public class Clusterer {
            public double[][] centroids;
            public int[] centAssign { get; set; } //Here. So, not 2 million records or anything....(..?)
            public int numCentroids { get; set; }
            int[] clustering;
            private List<myClusterable> _data;
            public List<myClusterable> Data {
                get { return _data; }
                set { _data = value; _dataCount = _data.Count; _fieldCount = Data[0]._item.FieldCount(); }
            }
            private int _dataCount;
            private int _fieldCount;

            public int[] Cluster() {
                var retVal = new int[_dataCount];
                clustering = new int[_dataCount];
                centroids = new double[numCentroids][];
                for (int i = 0; i < numCentroids; i++)
                    centroids[i] = new double[_fieldCount];

                AssignItems();
                Begin(10);
                while (!Finished && !Thrashing) {
                    CalculateCentroids();
                    Finished = !ReAssignItems();
                }
                Array.Copy(clustering, retVal, clustering.Length);
                return retVal;
            }

            private void CalculateCentroids() {

                int[] clusterCounts = new int[numCentroids];
                int clusterID = 0;
                for (int i = 0; i < _dataCount; i++) {
                    clusterID = clustering[i];
                    ++clusterCounts[clusterID];
                }
                //Still gonna be averages then.
                for (int k = 0; k < centroids.Length; k++)
                    for (int j = 0; j < centroids[k].Length; j++)
                        centroids[k][j] = 0.0;
                for (int i = 0; i < _dataCount; i++) {
                    clusterID = clustering[i];
                    for (int j = 0; j < _fieldCount; j++) {
                        centroids[clusterID][j] += Data[i]._item.FieldFuncs[j].Quantify();
                    }
                }
                for (int i = 0; i < numCentroids; i++) {
                    for (int j = 0; j < _fieldCount; j++) {
                        if (clusterCounts[i] == 0) {
                            centroids[i][j] = 0;
                        } else {
                            centroids[i][j] /= clusterCounts[i];
                        }
                    }
                }
            }

            private void AssignItems() {
                int clusterID = 0;
                for (int i = 0; i < _dataCount; i++) {
                    clustering[i] = clusterID++;
                    if (clusterID == numCentroids) clusterID = 0;
                }
            }

            private bool ReAssignItems() {
                bool changed = false;
                int clusterID = 0;
                int[] newClustering = new int[clustering.Length];
                double[] distances = new double[numCentroids];

                Array.Copy(clustering, newClustering, clustering.Length);
                for (int i = 0; i < _dataCount; i++) {
                    for (int j = 0; j < numCentroids; j++)
                        distances[j] = GetDistance(Data[i], centroids[j]);

                    int newClusterID = MinIndex(distances);
                    if (newClusterID != newClustering[i]) {
                        changed = true;
                        newClustering[i] = newClusterID;
                    }
                }
                if (!changed)
                    return false;

                int[] clusterCounts = new int[numCentroids];
                for (int i = 0; i < _dataCount; i++) {
                    clusterID = newClustering[i];
                    ++clusterCounts[clusterID];
                }
                for (int i = 0; i < numCentroids; i++) {
                    if (clusterCounts[i] == 0)
                        return false;
                }

                Array.Copy(newClustering, clustering, clustering.Length);
                return changed;
            }

            private double GetDistance(myClusterable data, double[] centroid) {
                double sumSqDiffs = 0.0;
                for (int i = 0; i < _fieldCount; i++)
                    sumSqDiffs += (data._item.FieldFuncs[i].Quantify() - centroid[i]) *
                                  (data._item.FieldFuncs[i].Quantify() - centroid[i]);
                return Math.Sqrt(sumSqDiffs);
            }

            private int MinIndex(double[] distances) {
                int iMin = 0;
                double smallest = distances[0];
                for (int i = 0; i < distances.Length; i++) {
                    if (distances[i] < smallest) {
                        smallest = distances[i];
                        iMin = i;
                    }
                }
                return iMin;
            }

            #region waffle
            //IComparable
            //IEquatable
            //..
            //IJudgeable
            //IReasonable
            //IPlaceable
            //IPutable
            //IOrientable
            //IArrangeable
            //
            //IPositionable
            //ISpaceable
            //IMeasurable
            //IEstimatable
            //ICentroidable
            //IClusterable
            //IPinPointable
            //IDefineable
            //IValueable
            //IEvaluateable
            //IClassifyable
            //IDeterminable
            //IAssignable
            //IDiscernable
            //ICalculatable
            //IInspectable
            //IWorkOutable
            //IDetectable
            //IAscertainable
            //IQueryable (not)
            //IVectorable
            //IScalarable
            //INumerifiable
            //IEnumerable (?)
            //IQuantifiable
            #endregion

            #region utilities
            private int _thrashing = Int32.MinValue;
            private int _threshold = 10;
            private void Begin(int threshold) { Finished = false; _thrashing = 0; _threshold = threshold < 1 ? Int32.MinValue : threshold; }
            public bool Thrashing { get => ++_thrashing > _threshold; }
            public bool Finished { get; set; } = false;
            #endregion
        }
    }
}

//################################################################################################

public interface IClusterable {
    decimal CentroidVal();
    int FieldCount();
    IQuantifiable[] FieldFuncs { get; }
}
public interface IQuantifiable { double Quantify(); }


//################################################################################################

public class fieldFuncInt : IQuantifiable {
    public gonnaBeClusteredOne GbcOne { get; set; }
    public fieldFuncInt(gonnaBeClusteredOne gbcOne) {
        GbcOne = gbcOne;
    }
    public double Quantify() { return GbcOne._num; }
}
public class fieldFuncStringOne : IQuantifiable {
    public gonnaBeClusteredOne GbcOne { get; set; }
    public fieldFuncStringOne(gonnaBeClusteredOne gbcOne) {
        GbcOne = gbcOne;
    }
    public double Quantify() { return double.Parse(GbcOne._text); }
}
public class fieldFuncStringToo : IQuantifiable {
    public gonnaBeClusteredToo GbcToo { get; set; }
    public fieldFuncStringToo(gonnaBeClusteredToo gbcToo) {
        GbcToo = gbcToo;
    }
    public double Quantify() { return double.Parse(GbcToo._num); }
}
public class fieldFuncDouble : IQuantifiable {
    public gonnaBeClusteredToo GbcToo { get; set; }
    public fieldFuncDouble(gonnaBeClusteredToo gbcToo) {
        GbcToo = gbcToo;
    }
    public double Quantify() { return GbcToo._text; }
}

//----------------------------------------------------

public class myClusterable {
    public IClusterable _item { get; set; }
    public myClusterable(IClusterable item) => _item = item;
}

//################################################################################################

public class gbcOneWrap : IClusterable {
    public gbcOneWrap(gonnaBeClusteredOne gbcOne) {
        _gbcOne = gbcOne;
        _fieldFuncs = FieldFuncs;
        _fieldFuncs[0] = new fieldFuncInt(_gbcOne);
        _fieldFuncs[1] = new fieldFuncStringOne(_gbcOne);
    }
    public gonnaBeClusteredOne _gbcOne { get; set; }
    public int FieldCount() => 2;
    private IQuantifiable[] _fieldFuncs = new IQuantifiable[2];
    public IQuantifiable[] FieldFuncs { get => _fieldFuncs; }
    public decimal CentroidVal() => decimal.Parse(_gbcOne._text);
}
public class gbcTooWrap : IClusterable {
    public gbcTooWrap(gonnaBeClusteredToo gbcToo) {
        _gbcToo = gbcToo;
        _fieldFuncs[0] = new fieldFuncStringToo(_gbcToo);
        _fieldFuncs[1] = new fieldFuncDouble(_gbcToo);
    }
    public gonnaBeClusteredToo _gbcToo { get; set; }
    public int FieldCount() => 2;
    private IQuantifiable[] _fieldFuncs = new IQuantifiable[2];
    public IQuantifiable[] FieldFuncs { get => _fieldFuncs; }
    public decimal CentroidVal() => decimal.Parse(_gbcToo._num);
}

//################################################################################################

public class gonnaBeClusteredOne { //Could be literally anything
    public int _num { get; set; }
    public string _text { get; set; }
    public object _obj { get; set; }
    public gonnaBeClusteredOne(int num, string text, object obj) {
        _num = num; _text = text; _obj = obj;
    }
}
public class gonnaBeClusteredToo { //Could be literally anything
    public string _num { get; set; }
    public double _text { get; set; }
    public object _obj { get; set; }
    public gonnaBeClusteredToo(string num, double text, object obj) {
        _num = num; _text = text; _obj = obj;
    }
}