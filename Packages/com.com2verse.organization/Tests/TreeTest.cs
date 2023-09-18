using System.Collections.Generic;
using System.Text;
using Com2Verse.Logger;
using Com2Verse.Organization;
using NUnit.Framework;

namespace Com2VerseTests.Organization
{
    /// <summary>
    /// 계층 트리 테스트
    /// </summary>
    public class TreeTest
    {
        enum eTraversalType
        {
            FORWARD,
            BACKWARD,
            ASCENT,
            DESCENT
        }

#region Test Data
        private readonly TreeData<string> _treeDatas = new TreeData<string>
        {
            Data = "Root",
            Childs = new []
            {
                new TreeData<string>
                {
                    Data = "1",
                    Childs = new[]
                    {
                        new TreeData<string>
                        {
                            Data = "1-1",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "1-1-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-1-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-1-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-1-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "1-2",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "1-2-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-2-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-2-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-2-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "1-3",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "1-3-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-3-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-3-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-3-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "1-4",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "1-4-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-4-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-4-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "1-4-4",
                                },
                            }
                        },
                    }
                },
                new TreeData<string>()
                {
                    Data = "2",
                    Childs = new[]
                    {
                        new TreeData<string>
                        {
                            Data = "2-1",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "2-1-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-1-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-1-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-1-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "2-2",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "2-2-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-2-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-2-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-2-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "2-3",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "2-3-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-3-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-3-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-3-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "2-4",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "2-4-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-4-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-4-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "2-4-4",
                                },
                            }
                        },
                    }
                },
                new TreeData<string>()
                {
                    Data = "3",
                    Childs = new[]
                    {
                        new TreeData<string>
                        {
                            Data = "3-1",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "3-1-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-1-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-1-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-1-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "3-2",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "3-2-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-2-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-2-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-2-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "3-3",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "3-3-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-3-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-3-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-3-4",
                                },
                            }
                        },
                        new TreeData<string>
                        {
                            Data = "3-4",
                            Childs = new[]
                            {
                                new TreeData<string>
                                {
                                    Data = "3-4-1",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-4-2",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-4-3",
                                },
                                new TreeData<string>
                                {
                                    Data = "3-4-4",
                                },
                            }
                        },
                    }
                },
                new TreeData<string>()
                {
                    Data = "4",
                    Childs = new[]
                    {
                        new TreeData<string>
                        {
                            Data = "4-1",
                        },
                        new TreeData<string>
                        {
                            Data = "4-2",
                        },
                        new TreeData<string>
                        {
                            Data = "4-3",
                        },
                        new TreeData<string>
                        {
                            Data = "4-4",
                        },
                    }
                },
            }
        };
        private class TreeData<T>
        {
            public T Data;
            public TreeData<T>[] Childs;
        }
#endregion // Test Data

#region Test Factor
        private readonly int _forwardTestIdx = 22;
        private readonly int _backwardTestIdx = 42;
        private readonly int _ascentTestIdx = 63;
        private readonly int _descentTestIdx = 22;

        private ResultInfo _expectForwardTest = new ResultInfo
        {
            TraversalCount = 47,
            TraversalOrder = new List<int>
            {
                22, 23, 24, 25, 26, 27, 28, 29,
                30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
                40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
                50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
                60, 61, 62, 63, 64, 65, 66, 67, 68,
            },
        };

        private ResultInfo _expectBackwardTest = new ResultInfo
        {
            TraversalCount = 43,
            TraversalOrder = new List<int>
            {
                42, 41, 40,
                39, 38, 37, 36, 35, 34, 33, 32, 31, 30,
                29, 28, 27, 26, 25, 24, 23, 22, 21, 20,
                19, 18, 17, 16, 15, 14, 13, 12, 11, 10,
                9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
            },
        };

        private ResultInfo _expectAscentTest = new ResultInfo
        {
            TraversalCount = 4,
            TraversalOrder = new List<int>
            {
                63, 59, 43, 0,
            },
        };

        private ResultInfo _expectDescentTest = new ResultInfo
        {
            TraversalCount = 21,
            TraversalOrder = new List<int>
            {
                22, 23, 24, 25, 26, 27, 28, 29, 30,
                31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
                41, 42
            },
        };
#endregion // Test Factor

#region Tests
        /// <summary>
        /// 테스트용 트리 출력
        /// </summary>
        [Test]
        public void PrintTestTree() => TraversalTest(0, eTraversalType.FORWARD, null);
        [Test]
        public void ForwardTraversalTest() => TraversalTest(_forwardTestIdx, eTraversalType.FORWARD, _expectForwardTest);
        [Test]
        public void BackwardTraversalTest() => TraversalTest(_backwardTestIdx, eTraversalType.BACKWARD, _expectBackwardTest);
        [Test]
        public void AscentTraversalTest() => TraversalTest(_ascentTestIdx, eTraversalType.ASCENT, _expectAscentTest);
        [Test]
        public void DescentTraversalTest() => TraversalTest(_descentTestIdx, eTraversalType.DESCENT, _expectDescentTest);
#endregion // Tests

#region Functions
        private void TraversalTest(int startIdx, eTraversalType traversalType, ResultInfo expect)
        {
            var root = CreateTestTree();
            var startNode = HierarchyTree<string>.FindByIndex(root, startIdx);

            IEnumerable<HierarchyTree<string>> enumerable = null;
            enumerable = GetEnumerator(startNode, traversalType);
            Assert.IsNotNull(enumerable);

            if (Config.DisplayLOG)
            {
                PrintTreeIterate(enumerable);
                enumerable = GetEnumerator(startNode, traversalType);
            }

            var result = DoTest(enumerable, expect);
            result.Print();

            if (expect != null)
                Assert.IsTrue(result.IsValid());
        }
        private HierarchyTree<string> CreateTestTree()
        {
            var root = HierarchyTree<string>.CreateNew(_treeDatas.Data);
            FillTree(root, _treeDatas);
            root.SetItemIndex();

            void FillTree<T>(HierarchyTree<T> tree, TreeData<T> treeData)
            {
                if (treeData?.Childs == null) return;

                foreach (var child in treeData.Childs)
                {
                    tree.AddChildren(child.Data);
                    var subTree = tree.LastChildren;
                    FillTree<T>(subTree, child);
                }
            }

            return root;
        }
        private IEnumerable<HierarchyTree<string>> GetEnumerator(HierarchyTree<string> startNode, eTraversalType traversalType)
        {
            switch (traversalType)
            {
                case eTraversalType.FORWARD:
                    return startNode.GetForwardEnumerator();
                case eTraversalType.BACKWARD:
                    return startNode.GetBackwardEnumerator();
                case eTraversalType.ASCENT:
                    return startNode.GetAscentEnumerator();
                case eTraversalType.DESCENT:
                    return startNode.GetDescentEnumerator();
                default:
                    return null;
            } 
        }
        private TestResult DoTest(IEnumerable<HierarchyTree<string>> enumerable, ResultInfo expect)
        {
            var result = new TestResult(expect);
            foreach (var node in enumerable)
            {
                result.Result.TraversalCount++;
                result.Result.TraversalOrder.Add(node.Index);
            }
            return result;
        }
        private void PrintTreeIterate(IEnumerable<HierarchyTree<string>> enumerable)
        {
            var sb = new StringBuilder();
            foreach (var node in enumerable)
            {
                for (var i = 0; i < node.Depth; ++i)
                    sb.Append("\t");
                sb.AppendLine($"[{node.Index}] {node.Value} ({node.Length})");
            }
            C2VDebug.Log(sb.ToString());
        }
#endregion // Functions

#region Test Result
        private class TestResult
        {
            public ResultInfo Result;
            public ResultInfo Expect; 
            public TestResult(ResultInfo expect)
            {
                Result = ResultInfo.CreateNew();
                Expect = expect;
            }

            public bool IsValid()
            {
                if (Result == null || Expect == null)
                {
                    if (Config.DisplayLOG)
                        C2VDebug.Log($"Result is NULL ({Result == null}) or Expect is NULL ({Expect == null})");
                    return false;
                }

                if (Result.TraversalCount != Expect.TraversalCount)
                {
                    if (Config.DisplayLOG)
                        C2VDebug.Log($"Traversal Count are not equal Result/Expect ({Result.TraversalCount} / {Expect.TraversalCount})");
                    return false;
                }

                if (Result.TraversalOrder.Count != Expect.TraversalOrder.Count)
                {
                    if (Config.DisplayLOG)
                        C2VDebug.Log($"TraverserOrder Count are not equal Result/Expect ({Result.TraversalOrder.Count} / {Expect.TraversalOrder.Count})");
                    return false;
                }
                for (var i = 0; i < Result.TraversalOrder.Count; i++)
                {
                    if (Result.TraversalOrder[i] != Expect.TraversalOrder[i])
                    {
                        if (Config.DisplayLOG)
                            C2VDebug.Log($"Validation Failed [{i}] Result/Expect ({Result.TraversalOrder[i]} / {Expect.TraversalOrder[i]})");
                        return false;
                    }
                }

                return true;
            }
            public void Print()
            {
                C2VDebug.Log($"========== TEST RESULT ==========");
                C2VDebug.Log($"Total Count : {Result.TraversalCount}");
                // foreach (var i in TraversalOrder)
                //     C2VDebug.Log(Convert.ToString(i));
            }
        }

        private class ResultInfo
        {
            public int TraversalCount;
            public List<int> TraversalOrder;

            public static ResultInfo CreateNew() => new ResultInfo 
            {
                TraversalOrder = new List<int>(),
                TraversalCount = 0,
            };
        }
#endregion // Test Result
    }
}
