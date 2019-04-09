Imports System

' For loops and creating new arrays. THAT'S ALL. (so far)

Namespace Hue_VB
    Module K_Means
        Function nl() As String
            Return Environment.NewLine()
        End Function
        Public Sub Main_K_Means(args As String())
            Console.WriteLine(nl() + "Begin k-means clustering demo" + nl())

            Dim rawData = New Double(9)() {}
            rawData(0) = New Double() {73, 72.6}
            rawData(1) = New Double() {61, 54.4}
            rawData(2) = New Double() {67, 99.9}
            rawData(3) = New Double() {68, 97.3}
            rawData(4) = New Double() {62, 59.0}
            rawData(5) = New Double() {75, 81.6}
            rawData(6) = New Double() {74, 77.1}
            rawData(7) = New Double() {66, 97.3}
            rawData(8) = New Double() {68, 93.3}
            rawData(9) = New Double() {61, 59.0}

            Console.WriteLine("Raw unclustered height (in.) weight (kg.) data:" + nl())
            Console.WriteLine(" ID   Height   Weight")
            Console.WriteLine("----------------------")
            ShowData(rawData, 1, True, False)

            Dim numClusters As Integer = 3
            Console.WriteLine(nl() + "Setting numClusters to " + numClusters.ToString())

            Console.WriteLine("Starting clustering using k-means algorithm")
            Dim c = New Clusterer(numClusters)
            Dim clustering As Integer() = c.Cluster(rawData)
            Console.WriteLine("Clustering complete" + nl())

            Console.WriteLine("Final clustering in internal form:" + nl())
            ShowVector(clustering, True)

            Console.WriteLine("Raw data by cluster:" + nl())
            ShowClustered(rawData, clustering, numClusters, 1)

            Console.WriteLine(nl() + "End k-means clustering demo")
            Console.ReadLine()
        End Sub
        Sub ShowData(data As Double()(), decimals As Integer, indices As Boolean, newLine As Boolean)
            Dim v As Double
            For i = 0 To data.Length - 1
                If indices Then
                    Console.WriteLine(i.ToString().PadLeft(3) + "  ")
                End If
                For j = 0 To data(i).Length - 1
                    v = data(i)(j)
                    Console.Write(v.ToString("F" + decimals.ToString()) + "    ")
                    Console.Write("")
                Next
                If newLine Then
                    Console.WriteLine("")
                End If
            Next
        End Sub
        Sub ShowVector(vector As Integer(), newLine As Boolean)
            For i = 0 To vector.Length - 1
                Console.Write(vector(i).ToString() + " ")
            Next
            If newLine Then
                Console.Write(nl() + "")
            End If
        End Sub
        Sub ShowClustered(data As Double()(), clustering As Integer(), numClusters As Integer, decimals As Integer)
            Dim clusterID As Integer
            Dim v As Double
            For k = 0 To numClusters - 1
                Console.WriteLine("====================")
                For i = 0 To data.Length - 1
                    clusterID = clustering(i)
                    If clusterID <> k Then
                        Continue For
                    End If
                    Console.Write(i.ToString().PadLeft(3) + "  ")
                    For j = 0 To data(i).Length - 1
                        v = data(i)(j)
                        Console.Write(v.ToString("F" + decimals.ToString()) + "    ")
                    Next
                    Console.WriteLine("")
                Next
            Next
            Console.WriteLine("====================")
        End Sub

        Class Clusterer
            Dim numClusters As Integer  ' Number of clusters
            Dim clustering As Integer() ' Index = a tuple, value = a cluster
            Dim centroids As Double()() ' Mean (vector) of each cluster
            Dim rnd As Random           ' For initialisation

            Sub New(numClusters As Integer)
                Me.numClusters = numClusters
                centroids = New Double(numClusters - 1)() {}
                rnd = New Random(0) ' arbitrary seed
            End Sub

            Function Cluster(data As Double()()) As Integer()
                Dim numTuples = data.Length - 1
                Dim numValues = data(0).Length - 1
                clustering = New Integer(numTuples) {}
                For k = 0 To numClusters - 1
                    centroids(k) = New Double(numValues) {}
                Next
                InitRandomData(data)

                Console.WriteLine(nl() + "Initial random clustering:")
                For i = 0 To clustering.Length - 1
                    Console.Write(clustering(i).ToString() + " ")
                Next
                Console.WriteLine("")

                Dim changed As Boolean = True ' Change in clustering?
                Dim maxCount As Integer = numTuples * 10 ' sanity check
                Dim ct As Integer = 0
                While changed And ct <= maxCount
                    ct += 1 ' k-means typically converges very quickly
                    UpdateCentroids(data) ' No effect if fail
                    changed = UpdateClustering(data) ' No effect if fail
                End While

                Dim result = New Integer(numTuples) {}
                Array.Copy(clustering, result, clustering.Length)
                Cluster = result 'return result
            End Function

            Sub InitRandomData(data As Double()())
                Dim numTuples As Integer = data.Length - 1
                Dim clusterID As Integer = -1
                Dim r As Integer
                Dim tmp As Integer
                For i = 0 To numTuples
                    clusterID += 1
                    clustering(i) = clusterID
                    If clusterID = numClusters - 1 Then
                        clusterID = -1
                    End If
                Next
                'For i = 0 To numTuples
                '    r = rnd.Next(i, clustering.Length)
                '    tmp = clustering(r)
                '    clustering(r) = clustering(i)
                '    clustering(i) = tmp
                'Next
            End Sub

            Sub UpdateCentroids(data As Double()())
                Dim clusterCounts = New Integer(numClusters) {}
                Dim clusterID As Integer
                For i = 0 To data.Length - 1
                    clusterID = clustering(i)
                    clusterCounts(clusterID) += 1
                Next

                'Zero-out centroids so it can be used as scratch
                For k = 0 To centroids.Length - 1
                    For j = 0 To centroids(k).Length - 1
                        centroids(k)(j) = 0.0
                    Next
                Next
                For i = 0 To data.Length - 1
                    clusterID = clustering(i)
                    For j = 0 To data(i).Length - 1
                        centroids(clusterID)(j) += data(i)(j) ' accumulate sum
                    Next
                Next
                For k = 0 To centroids.Length - 1
                    For j = 0 To centroids(k).Length - 1
                        centroids(k)(j) /= clusterCounts(k) ' danger?
                    Next
                Next
            End Sub

            Function UpdateClustering(data As Double()()) As Boolean
                ' (re)assign each tuple to a cluster (closest centroid)
                ' returns flase if no tuple assignments change OR
                ' if the reassignment would result in a clustering where
                ' one or more clusters have no tuples

                Dim changed As Boolean = False 'did any tuple change cluster?
                Dim clusterID As Integer
                Dim newClustering = New Integer(clustering.Length - 1) {} ' proposed result
                Array.Copy(clustering, newClustering, clustering.Length)

                Dim distances = New Double(numClusters - 1) {} ' from tuple to centroids
                Dim newClusterID As Integer
                For i = 0 To data.Length - 1
                    For k = 0 To numClusters - 1
                        distances(k) = Distance(data(i), centroids(k))
                    Next

                    newClusterID = MinIndex(distances) ' find closest centroid
                    If newClusterID <> newClustering(i) Then
                        changed = True ' note a new clustering
                        newClustering(i) = newClusterID ' accept update
                    End If
                Next

                If Not changed Then
                    UpdateClustering = False 'Return False
                End If

                ' check proposed clustering cluster counts
                Dim clusterCounts = New Integer(numClusters) {}
                For i = 0 To data.Length - 1
                    clusterID = newClustering(i)
                    clusterCounts(clusterID) += 1
                Next

                For k = 0 To numClusters
                    If clusterCounts(k) = 0 Then
                        UpdateClustering = False 'Return False ' bad clustering
                    End If
                Next

                ' alternative: place a random data item into empty cluster
                'Dim cid As Integer
                'Dim ct As Integer
                'For k = 0 To numClusters
                '    If clusterCounts(k) = 0 Then ' cluster k has no items
                '        For t = 0 To data.Length ' find a tuple to put into cluster k
                '            cid = newClustering(t)  ' cluster of t
                '            ct = clusterCounts(cid) ' how many more items?
                '            If ct >= 2 Then ' t is in a cluster w/ 2 or more items
                '                newClustering(t) = k  ' place t into cluster k
                '                clusterCounts(k) += 1 ' k now has a data item
                '                clusterCounts(cid) -= 1 ' cluster that used to have t
                '                break 'or something
                '            End If
                '        Next
                '    End If
                'Next

                Array.Copy(newClustering, clustering, newClustering.Length)
                UpdateClustering = True 'return true
            End Function

            Function Distance(tuple As Double(), centroid As Double()) As Double
                ' Euclidean distance between two vectors for UpdateClustering()
                Dim sumSqDiffs = 0.0
                For j = 0 To tuple.Length - 1
                    sumSqDiffs += (tuple(j) - centroid(j)) * (tuple(j) - centroid(j))
                Next
                Distance = Math.Sqrt(sumSqDiffs) 'return Math.Sqrt(sumSqDiffs)
            End Function

            Function MinIndex(distances As Double()) As Double
                ' helper for UpdateClustering() to find closest centroid
                Dim indexOfMin As Integer = 0
                Dim smallDist As Integer = distances(0)
                For k = 0 To distances.Length - 1
                    If distances(k) < smallDist Then
                        smallDist = distances(k)
                        indexOfMin = k
                    End If
                Next
                MinIndex = indexOfMin ' return indexOfMin
            End Function
        End Class
    End Module
    Module GACUK
        Function nl() As String
            Return Environment.NewLine()
        End Function
        Public Sub Main(args As String())

            Console.WriteLine(nl() + "Begin categorical data clustering demo" + nl())

            Dim rawData = New String(6)() {}

            rawData(0) = New String() {"Blue", "Small", "False"}
            rawData(1) = New String() {"Green", "Medium", "True"}
            rawData(2) = New String() {"Red", "Large", "False"}
            rawData(3) = New String() {"Red", "Small", "True"}
            rawData(4) = New String() {"Green", "Medium", "False"}
            rawData(5) = New String() {"Yellow", "Medium", "False"}
            rawData(6) = New String() {"Red", "Large", "False"}

            Console.WriteLine("Raw unclustered data:" + nl())
            Console.WriteLine("   Color   Size   Heavy")
            Console.WriteLine("-------------------------")
            ShowData(rawData)

            Dim numClusters As Integer = 2
            Console.WriteLine(nl() + "Setting numClusters to " + numClusters.ToString())
            Dim numRestarts As Integer = 4 'Sqrt(Data.Length)
            Console.WriteLine("Setting numRestarts to " + numRestarts.ToString())

            Console.WriteLine(nl() + "Starting clustering using greedy agglomerative category utility")
            Dim cc = New CatClusterer(numClusters, rawData) ' restart version
            Dim cu As Double
            Dim clustering As Integer() = cc.Cluster(numRestarts, cu) 'ByRef the cu 
            Console.WriteLine("Clustering complete" + nl())

            Console.WriteLine("Final clustering in internal form:")
            ShowVector(clustering, True)

            Console.WriteLine("Final CU value = " + cu.ToString("F4"))

            Console.WriteLine(nl() + "Raw data grouped by cluster" + nl())
            ShowClustering(numClusters, clustering, rawData)

            Console.WriteLine(nl() + "End categorical data clustering demo" + nl())
            Console.ReadLine()
        End Sub

        Sub ShowData(matrix As String()()) ' for tuples
            For i = 0 To matrix.Length - 1
                Console.Write("[" + i.ToString() + "] ")
                For j = 0 To matrix(i).Length - 1
                    Console.Write(matrix(i)(j).ToString().PadRight(8) + " ")
                Next
                Console.WriteLine("")
            Next
        End Sub
        Sub ShowVector(vector As Integer(), newLine As Boolean) ' for clustering
            For i = 0 To vector.Length - 1
                Console.Write(vector(i).ToString() + " ")
            Next
            Console.WriteLine("")
            If newLine Then
                Console.WriteLine("")
            End If
        End Sub
        Sub ShowClustering(numClusters As Integer, clustering As Integer(), rawData As String()())
            Console.WriteLine("---------------------------------")
            For k = 0 To numClusters - 1 ' display by cluster
                For i = 0 To rawData.Length - 1 ' each tuple
                    If clustering(i) = k Then ' curr tuple i belongs to curr cluster k
                        Console.Write(i.ToString().PadLeft(2) + "  ")
                        For j = 0 To rawData(i).Length - 1
                            Console.Write(rawData(i)(j).ToString().PadRight(8) + " ")
                        Next
                        Console.WriteLine("")
                    End If
                Next
                Console.WriteLine("---------------------------------")
            Next
        End Sub

        Public Class CatClusterer
            Dim numClusters As Integer ' number of clusters
            Dim clustering As Integer() ' index = a tuple, value = a cluster ID
            Dim dataAsInts As Integer()() ' ex: Red = 0, blue = 1, green = 2
            Dim valueCounts As Integer()()() ' scratch to compute CU [att][va][count]
            Dim clusterCounts As Integer() ' number tuples assigned to each cluster (sum)
            Dim rnd As Random

            Sub New(numClusters As Integer, rawData As String()())
                Me.numClusters = numClusters
                MakeDataMatrix(rawData) ' convert strings to ints into this.dataAsInts()()
                Allocate() ' allocate all arrays and matrices (no initialise values)
            End Sub

            Function Cluster(numRestarts As Integer, ByRef catUtility As Double) As Integer()
                ' restart version
                Dim numRows As Integer = dataAsInts.Length
                Dim currCU As Double = 0.0
                Dim bestCU As Double = 0.0
                Dim bestClustering = New Integer(numRows - 1) {}
                For start = 0 To numRestarts - 1
                    Dim seed As Integer = start ' use the start index as rnd seed
                    Dim currClustering As Integer() = ClusterOnce(seed, currCU)
                    If currCU > bestCU Then
                        bestCU = currCU
                        Array.Copy(currClustering, bestClustering, numRows)
                    End If
                Next
                catUtility = bestCU
                Cluster = bestClustering
            End Function

            Function ClusterOnce(seed As Integer, ByRef catUtility As Double) As Integer()
                rnd = New Random
                Initialise() ' clustering() to -1, all counts() to 0

                Dim numTrials As Integer = dataAsInts.Length ' for initial tuple assignments
                Dim goodIndexes As Integer() = GetGoodIndices(numTrials) ' tuples that are dissimilar
                For k = 0 To numClusters - 1 ' assign first tuples to clusters
                    Assign(goodIndexes(k), k)
                Next

                Dim numRows As Integer = dataAsInts.Length
                Dim rndSequence = New Integer(numRows - 1) {}
                For i = 0 To numRows - 1
                    rndSequence(i) = i
                Next
                Shuffle(rndSequence) ' present tuples in random sequence

                For t = 0 To numRows - 1 ' main loop. walk through each tuple
                    Dim idx As Integer = rndSequence(t) ' index of data tuple to process
                    If clustering(idx) <> -1 Then ' tuple clustered by initialisation
                        Continue For
                    End If
                    Dim candidateCU = New Double(numClusters - 1) {} ' candidate CU values

                    For k = 0 To numClusters - 1
                        Assign(idx, k) ' tentative cluster assignment
                        candidateCU(k) = CategoryUtility() ' compute and save the CU
                        Unassign(idx, k) ' undo tentative assignment
                    Next

                    Dim bestK As Integer = MaxIndex(candidateCU) ' greedy. the index is a clusterID
                    Assign(idx, bestK) ' now we know which cluster gave the best CU
                Next ' each tuple

                catUtility = CategoryUtility()
                Dim result = New Integer(numRows - 1) {}
                Array.Copy(clustering, result, numRows)
                ClusterOnce = result
            End Function

            Sub MakeDataMatrix(rawData As String()())
                Dim numRows As Integer = rawData.Length
                Dim numCols As Integer = rawData(0).Length

                dataAsInts = New Integer(numRows - 1)() {} ' allocate all
                For i = 0 To numRows - 1
                    dataAsInts(i) = New Integer(numCols - 1) {}
                Next
                Dim idx As Integer
                Dim v As Integer
                For col = 0 To numCols - 1
                    idx = 0
                    Dim dict = New Dictionary(Of String, Integer)()
                    For row = 0 To numRows - 1 ' build dict for curr col
                        Dim s As String = rawData(row)(col)
                        If Not dict.ContainsKey(s) Then
                            idx += 1
                            dict.Add(s, idx)
                        End If
                    Next
                    For row = 0 To numRows - 1
                        Dim s As String = rawData(row)(col)
                        v = dict(s)
                        dataAsInts(row)(col) = v
                    Next
                Next
            End Sub

            Sub Allocate()
                ' assumes dataAsInts has been created
                ' allocate clustering(), clusterCounts(), valueCounts()()()

                Dim numRows As Integer = dataAsInts.Length
                Dim numCols As Integer = dataAsInts(0).Length

                clustering = New Integer(numRows - 1) {}
                clusterCounts = New Integer(numClusters) {} ' last cell is sum (+1)

                valueCounts = New Integer(numCols - 1)()() {} '1st dim

                Dim maxVal As Integer
                For col = 0 To numCols - 1 ' need # distinct values in each col
                    maxVal = 0
                    For i = 0 To numRows - 1
                        If dataAsInts(i)(col) > maxVal Then
                            maxVal = dataAsInts(i)(col)
                        End If
                    Next
                    valueCounts(col) = New Integer(maxVal)() {} ' 0-based 2nd dim (+1)
                Next

                For i = 0 To valueCounts.Length - 1
                    For j = 0 To valueCounts(i).Length - 1
                        valueCounts(i)(j) = New Integer(numClusters) {} ' +1 last cell is sum (+1)
                    Next
                Next
            End Sub

            Sub Initialise()
                For i = 0 To clustering.Length - 1
                    clustering(i) = -1
                Next
                For i = 0 To clusterCounts.Length - 1
                    clusterCounts(i) = 0
                Next
                For i = 0 To valueCounts.Length - 1
                    For j = 0 To valueCounts(i).Length - 1
                        For k = 0 To valueCounts(i)(j).Length - 1
                            valueCounts(i)(j)(k) = 0
                        Next
                    Next
                Next
            End Sub

            Function CategoryUtility() As Double ' called by ClusterOnce
                ' because CU is called many times use precomputed counts
                Dim numTuplesAssigned As Integer = clusterCounts(clusterCounts.Length - 1) ' last cell (-1)

                Dim clusterProbs = New Double(numClusters - 1) {}
                For k = 0 To numClusters - 1
                    clusterProbs(k) = (clusterCounts(k) * 1.0) / numTuplesAssigned
                Next

                ' single unconditional prob term
                Dim unconditional As Double = 0.0
                Dim sum As Integer
                Dim p As Double
                For i = 0 To valueCounts.Length - 1
                    For j = 0 To valueCounts(i).Length - 1
                        sum = valueCounts(i)(j)(numClusters) ' last cell holds sum
                        p = (sum * 1.0) / numTuplesAssigned
                        unconditional += (p * p)
                    Next
                Next

                ' conditional terms each cluster
                Dim conditionals = New Double(numClusters - 1) {}
                For k = 0 To numClusters - 1
                    For i = 0 To valueCounts.Length - 1 ' each att
                        For j = 0 To valueCounts(i).Length - 1 ' each value
                            p = (valueCounts(i)(j)(k) * 1.0) / clusterCounts(k)
                            conditionals(k) += (p * p)
                        Next
                    Next
                Next

                ' we have P(Ck), EE P(Ai=Vij|Ck)^2, EE P(Ai=Vij)^2 so we can compute CU easily
                Dim summation As Double = 0.0
                For k = 0 To numClusters - 1
                    summation += clusterProbs(k) * (conditionals(k) - unconditional)
                    ' E P(Ck) * [EE P(Ai=Vij|Ck)^2 - EE P(Ai=Vij)^2] / n
                Next
                CategoryUtility = summation / numClusters
            End Function

            Function MaxIndex(cus As Double()) As Integer
                ' helper for ClusterOnce. returns index of largest value in array
                Dim bestCU As Double = 0.0
                Dim indexOfBestCU As Integer = 0
                For k = 0 To cus.Length - 1
                    If cus(k) > bestCU Then
                        bestCU = cus(k)
                        indexOfBestCU = k
                    End If
                Next
                MaxIndex = indexOfBestCU
            End Function

            Sub Shuffle(indices As Integer()) ' instance so can use class rnd
                Dim ri As Integer
                Dim tmp As Integer
                For i = 0 To indices.Length - 1 ' Fisher-Yates shuffle
                    ri = rnd.Next(i, indices.Length) ' random index
                    tmp = indices(i)
                    indices(i) = indices(ri) ' swap
                    indices(ri) = tmp
                Next
            End Sub

            Sub Assign(dataIndex As Integer, clusterID As Integer)
                ' assign tuple at dataIndex to clustering() cluster, and
                ' update valueCounts()()(), clusterCounts()
                clustering(dataIndex) = clusterID  ' assign

                Dim v As Integer
                For i = 0 To valueCounts.Length - 1 ' update valueCounts
                    v = dataAsInts(dataIndex)(i) ' att value
                    valueCounts(i)(v)(clusterID) += 1 ' bump count
                    valueCounts(i)(v)(numClusters) += 1 ' bump sum)
                Next
                clusterCounts(clusterID) += 1 ' update clusterCounts
                clusterCounts(numClusters) += 1 ' last cell is sum
            End Sub

            Sub Unassign(dataIndex As Integer, clusterID As Integer)
                clustering(dataIndex) = -1 ' unassign
                Dim v As Integer
                For i = 0 To valueCounts.Length - 1 ' update
                    v = dataAsInts(dataIndex)(i)
                    valueCounts(i)(v)(clusterID) -= 1
                    valueCounts(i)(v)(numClusters) -= 1 ' last cell is sum
                Next
                clusterCounts(clusterID) -= 1 ' update clusterCounts
                clusterCounts(numClusters) -= 1 ' last cell
            End Sub

            Function GetGoodIndices(numTrials As Integer) As Integer()
                ' return numclusters indices of tuples that are different
                Dim numRows As Integer = dataAsInts.Length
                Dim numCols As Integer = dataAsInts(0).Length
                Dim result = New Integer(numClusters - 1) {}

                Dim largestDiff As Integer = -1 ' Difference for a set of numClusters tuples
                Dim candidates As Integer()
                Dim numDifferences As Integer
                Dim aRow As Integer
                Dim bRow As Integer
                For trials = 0 To numTrials - 1
                    candidates = Reservoir(numClusters, numRows)
                    numDifferences = 0
                    For i = 0 To candidates.Length - 1 ' all possible pairs
                        For j = 0 To candidates.Length - 1
                            aRow = candidates(i)
                            bRow = candidates(j)

                            For col = 0 To numCols - 1
                                If dataAsInts(aRow)(col) <> dataAsInts(bRow)(col) Then
                                    numDifferences += 1
                                End If
                            Next
                        Next
                    Next

                    'For i = 0 To candidates.Length ' only adjacent pairs
                    '    aRow = candidates(i)
                    '    bRow = candidates(i + 1)
                    '    For col = 0 To numCols
                    '        If dataAsInts(aRow)(col) <> dataAsInts(bRow)(col) Then
                    '            numDifferences += 1
                    '        End If
                    '    Next
                    'Next

                    If numDifferences > largestDiff Then
                        largestDiff = numDifferences
                        Array.Copy(candidates, result, numClusters)
                    End If

                Next
                GetGoodIndices = result
            End Function

            Function Reservoir(n As Integer, range As Integer) As Integer() ' helper for GetGoodIndices
                ' select n random indices between [0, range)
                Dim result = New Integer(n - 1) {}
                For i = 0 To n - 1
                    result(i) = i
                Next
                Dim j As Integer
                For t = n To range - 1
                    j = rnd.Next(0, t)
                    If j < n Then
                        result(j) = t
                    End If
                Next
                Reservoir = result
            End Function
        End Class
    End Module
End Namespace
