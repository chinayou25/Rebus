﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus2.Persistence.InMem;
using Rebus2.Sagas;

namespace Tests.Contracts.Sagas
{
    [TestFixture]
    public class BasicOperations : FixtureBase
    {
        InMemorySagaStorage _sagaStorage;

        protected override void SetUp()
        {
            _sagaStorage = new InMemorySagaStorage();
        }

        [Test]
        public async Task GetsNullWhenNoInstanceMatches()
        {
            var data = await _sagaStorage.Find(typeof(TestSagaData), "CorrelationId", "whatever");

            Assert.That(data, Is.Null);
        }

        [Test]
        public async Task GetsNullWhenPropertyDoesNotExist()
        {
            var data = await _sagaStorage.Find(typeof(TestSagaData), "NonExistingCorrelationId", "whatever");

            Assert.That(data, Is.Null);
        }

        [Test]
        public async Task GetsNullWhenValueDoesNotExist()
        {
            await _sagaStorage.Insert(new TestSagaData { Id = Guid.NewGuid(), CorrelationId = "existing" });

            var data = await _sagaStorage.Find(typeof(TestSagaData), "CorrelationId", "non-existing");

            Assert.That(data, Is.Null);
        }

        [Test]
        public async Task GetsTheInstanceWhenCorrelationPropertyMatches()
        {
            var sagaId = Guid.NewGuid();

            await _sagaStorage.Insert(new TestSagaData { Id = sagaId, CorrelationId = "existing" });

            var data = await _sagaStorage.Find(typeof(TestSagaData), "CorrelationId", "existing");

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Id, Is.EqualTo(sagaId));
        }

        [Test]
        public async Task GetsNullWhenTheTypeDoesNotMatch()
        {
            var sagaId = Guid.NewGuid();

            await _sagaStorage.Insert(new TestSagaData { Id = sagaId, CorrelationId = "existing" });

            var data = await _sagaStorage.Find(typeof(AnotherSagaData), "CorrelationId", "existing");

            Assert.That(data, Is.Null);
        }

        [Test]
        public async Task GetsTheInstanceWhenIdPropertyMatches()
        {
            var sagaId = Guid.NewGuid();

            await _sagaStorage.Insert(new TestSagaData { Id = sagaId, CorrelationId = "existing" });

            var data = await _sagaStorage.Find(typeof(TestSagaData), "Id", sagaId);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Id, Is.EqualTo(sagaId));
        }

        [Test]
        public async Task NewlyInsertedSagaDataIsRevisionZero()
        {
            var sagaId = Guid.NewGuid();

            await _sagaStorage.Insert(new TestSagaData
            {
                Id = sagaId,
                Data = "yes, den kender jeg"
            });

            var loadedSagaData = await _sagaStorage.Find(typeof(TestSagaData), "Id", sagaId);

            Assert.That(loadedSagaData.Revision, Is.EqualTo(0));
        }

        [Test]
        public async Task RevisionIsIncrementedOnEachUpdate()
        {
            var sagaId = Guid.NewGuid();

            await _sagaStorage.Insert(new TestSagaData { Id = sagaId, Data = "yes, den kender jeg" });
            var loadedSagaData0 = await _sagaStorage.Find(typeof(TestSagaData), "Id", sagaId);
            Assert.That(loadedSagaData0.Revision, Is.EqualTo(0));

            await _sagaStorage.Update(loadedSagaData0);
            var loadedSagaData1 = await _sagaStorage.Find(typeof(TestSagaData), "Id", sagaId);
            Assert.That(loadedSagaData1.Revision, Is.EqualTo(1));

            await _sagaStorage.Update(loadedSagaData1);
            var loadedSagaData2 = await _sagaStorage.Find(typeof(TestSagaData), "Id", sagaId);
            Assert.That(loadedSagaData2.Revision, Is.EqualTo(2));
        }


        class TestSagaData : ISagaData
        {
            public Guid Id { get; set; }
            public int Revision { get; set; }
            public string CorrelationId { get; set; }
            public string Data { get; set; }
        }

        class AnotherSagaData : ISagaData
        {
            public Guid Id { get; set; }
            public int Revision { get; set; }
        }
    }
}