
using System;
using SortedObservableCollection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.ComponentModel;

namespace SortedObservableCollection
{
    class TestObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public TestObject()
        { }

        public TestObject(int testPropertyValue)
        {
            TestProperty = testPropertyValue;
        }

        private int testProperty;

        public int TestProperty
        {
            get
            {
                return testProperty;
            }

            set
            {
                if(value != testProperty)
                {
                    testProperty = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("TestProperty"));
                }
            }
        }
    }

    class CompareableTestObject : TestObject, IComparable
    {
        public CompareableTestObject(int testPropertyValue) : base(testPropertyValue)
        {
        }

        public int CompareTo(object obj)
        {
            return this.TestProperty.CompareTo(((TestObject)obj).TestProperty);
        }
    }

    [TestClass]
    public class SortedObservableCollectionUnitTests
    {
        [TestMethod]
        public void InitTest()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new SortedObservableCollection<TestObject>());

            Assert.IsNotNull(new SortedObservableCollection<CompareableTestObject>());
        }
        
        [TestMethod]
        public void TwoIntTest1()
        {
            SortedObservableCollection<int> collection = new SortedObservableCollection<int>();

            collection.Add(1);
            collection.Add(0);

            Assert.AreEqual(2, collection.Count);

            Assert.AreEqual(0, collection[0]);
            Assert.AreEqual(1, collection[1]);
        }

        [TestMethod]
        public void TwoTestObjectMoveTest1()
        {
            SortedObservableCollection<CompareableTestObject> collection = new SortedObservableCollection<CompareableTestObject>();

            collection.Add(new CompareableTestObject(5));
            collection.Add(new CompareableTestObject(3));

            Assert.AreEqual(2, collection.Count);

            Assert.AreEqual(3, collection[0].TestProperty);
            Assert.AreEqual(5, collection[1].TestProperty);

            collection[1].TestProperty = 2;

            Assert.AreEqual(2, collection[0].TestProperty);
            Assert.AreEqual(3, collection[1].TestProperty);
        }

        [TestMethod]
        public void TwoTestObjectNoMoveTest()
        {
            SortedObservableCollection<CompareableTestObject> collection = new SortedObservableCollection<CompareableTestObject>();

            collection.Add(new CompareableTestObject(5));
            collection.Add(new CompareableTestObject(3));

            Assert.AreEqual(2, collection.Count);

            Assert.AreEqual(3, collection[0].TestProperty);
            Assert.AreEqual(5, collection[1].TestProperty);

            collection[1].TestProperty = 7;

            Assert.AreEqual(3, collection[0].TestProperty);
            Assert.AreEqual(7, collection[1].TestProperty);
        }

        [TestMethod]
        public void ReverseIntTest()
        {
            SortedObservableCollection<int> collection = new SortedObservableCollection<int>();

            collection.Add(5);
            collection.Add(4);
            collection.Add(3);
            collection.Add(2);
            collection.Add(1);


            Assert.AreEqual(5, collection.Count);

            Assert.AreEqual(1, collection[0]);
            Assert.AreEqual(2, collection[1]);
            Assert.AreEqual(3, collection[2]);
            Assert.AreEqual(4, collection[3]);
            Assert.AreEqual(5, collection[4]);
        }

        [TestMethod]
        public void MixedIntTest()
        {
            SortedObservableCollection<int> collection = new SortedObservableCollection<int>();

            collection.Add(1);
            collection.Add(3);
            collection.Add(4);
            collection.Add(2);
            collection.Add(5);

            Assert.AreEqual(5, collection.Count);

            Assert.AreEqual(1, collection[0]);
            Assert.AreEqual(2, collection[1]);
            Assert.AreEqual(3, collection[2]);
            Assert.AreEqual(4, collection[3]);
            Assert.AreEqual(5, collection[4]);
        }

        [TestMethod]
        public void SameIntTest()
        {
            SortedObservableCollection<int> collection = new SortedObservableCollection<int>();

            collection.Add(1);
            collection.Add(1);
            collection.Add(1);
            collection.Add(1);
            collection.Add(1);

            Assert.AreEqual(5, collection.Count);

            Assert.AreEqual(1, collection[0]);
            Assert.AreEqual(1, collection[1]);
            Assert.AreEqual(1, collection[2]);
            Assert.AreEqual(1, collection[3]);
            Assert.AreEqual(1, collection[4]);
        }


        [TestMethod]
        public void ReverseIntWithStartEndRemoveTest()
        {
            SortedObservableCollection<int> collection = new SortedObservableCollection<int>();

            collection.Add(5);
            collection.Add(4);
            collection.Add(3);
            collection.Add(2);
            collection.Add(1);

            collection.Remove(5);
            collection.Remove(1);

            Assert.AreEqual(3, collection.Count);

            Assert.AreEqual(2, collection[0]);
            Assert.AreEqual(3, collection[1]);
            Assert.AreEqual(4, collection[2]);
        }

        [TestMethod]
        public void MixedTestObjectWithMoveRemoveStay()
        {
            SortedObservableCollection<CompareableTestObject> collection = new SortedObservableCollection<CompareableTestObject>();

            collection.Add(new CompareableTestObject(5));
            collection.Add(new CompareableTestObject(3));
            collection.Add(new CompareableTestObject(1));
            collection.Add(new CompareableTestObject(4));
            collection.Add(new CompareableTestObject(2));

            Assert.AreEqual(5, collection.Count);

            collection.Remove(collection[2]);   //1, 2, 4, 5
            collection.RemoveAt(2);             //1, 2, 5

            Assert.AreEqual(1, collection[0].TestProperty);
            Assert.AreEqual(2, collection[1].TestProperty);
            Assert.AreEqual(5, collection[2].TestProperty);

            collection[2].TestProperty = 5; //1, 2, 5

            Assert.AreEqual(1, collection[0].TestProperty);
            Assert.AreEqual(2, collection[1].TestProperty);
            Assert.AreEqual(5, collection[2].TestProperty);

            collection[2].TestProperty = 4; //1, 2, 4

            Assert.AreEqual(1, collection[0].TestProperty);
            Assert.AreEqual(2, collection[1].TestProperty);
            Assert.AreEqual(4, collection[2].TestProperty);

            collection[0].TestProperty = 0; //0, 2, 4

            Assert.AreEqual(0, collection[0].TestProperty);
            Assert.AreEqual(2, collection[1].TestProperty);
            Assert.AreEqual(4, collection[2].TestProperty);

            collection[2].TestProperty = 5; //0, 2, 5

            Assert.AreEqual(0, collection[0].TestProperty);
            Assert.AreEqual(2, collection[1].TestProperty);
            Assert.AreEqual(5, collection[2].TestProperty);

            collection.Add(new CompareableTestObject(1));
            collection.Add(new CompareableTestObject(3));
            collection.Add(new CompareableTestObject(5)); //0, 1, 2, 3, 5, 5

            Assert.AreEqual(0, collection[0].TestProperty);
            Assert.AreEqual(1, collection[1].TestProperty);
            Assert.AreEqual(2, collection[2].TestProperty);
            Assert.AreEqual(3, collection[3].TestProperty);
            Assert.AreEqual(5, collection[4].TestProperty);
            Assert.AreEqual(5, collection[5].TestProperty);

            collection[5].TestProperty = 4; //0, 1, 2, 3, 4, 5

            Assert.AreEqual(0, collection[0].TestProperty);
            Assert.AreEqual(1, collection[1].TestProperty);
            Assert.AreEqual(2, collection[2].TestProperty);
            Assert.AreEqual(3, collection[3].TestProperty);
            Assert.AreEqual(4, collection[4].TestProperty);
            Assert.AreEqual(5, collection[5].TestProperty);
        }

    }
}
