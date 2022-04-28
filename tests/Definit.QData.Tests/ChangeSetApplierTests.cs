using Definit.QData.ChangeSets;
using Definit.QData.Model;
using Moq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Definit.QData.Tests
{
    public class ChangeSetApplierTests
    {
        private class TestEntity
        {
            [Key]
            public int Id { get; set; }
        }

        [Theory]
        [InlineData(ChangeSetOperation.None)]
        [InlineData(ChangeSetOperation.Update)]
        [InlineData(ChangeSetOperation.Delete)]
        public void Apply_NotInsert_WithoutExistingEntity_ThrowsChangeSetException(ChangeSetOperation operation)
        {
            var stubChangeSetContext = new Mock<IChangeSetContext>();
            var stubQDataEntityModel = new Mock<IQDataEntityModel>();

            var changeSetApplier = new ChangeSetApplier<TestEntity>(stubChangeSetContext.Object, stubQDataEntityModel.Object);
            var testChangeSet = new ChangeSetEntry()
            {
                Operation = operation
            };

            void applyChangeSet() => changeSetApplier.ApplyChangeSet(testChangeSet);

            Assert.Throws<ChangeSetException>(applyChangeSet);
        }

        [Fact]
        public void Apply_None_AppliedEntitySameAsExisting()
        {
            var testEntity = new TestEntity()
            {
                Id = 1,
            };

            var stubChangeSetContext = new Mock<IChangeSetContext>();
            stubChangeSetContext.Setup(x => x.LoadEntity(typeof(TestEntity), It.IsAny<object[]>())).Returns(testEntity);

            var stubQDataEntityModel = new Mock<IQDataEntityModel>();

            var changeSetApplier = new ChangeSetApplier<TestEntity>(stubChangeSetContext.Object, stubQDataEntityModel.Object);

            var testChangeSet = new ChangeSetEntry()
            {
                Operation = ChangeSetOperation.None,
            };

            changeSetApplier.ApplyChangeSet(testChangeSet);
            var appliedEntity = changeSetApplier.AppliedChangeSetEntries[testChangeSet];

            Assert.Equal(testEntity, appliedEntity);
        }

        [Fact]
        public void Apply_Insert_AddsAppliedEntityToContext()
        {
            var mockChangeSetContext = new Mock<IChangeSetContext>();
            mockChangeSetContext.Setup(x => x.AddNewEntity(It.IsAny<TestEntity>()));
            var stubQDataEntityModel = new Mock<IQDataEntityModel>();

            var changeSetApplier = new ChangeSetApplier<TestEntity>(mockChangeSetContext.Object, stubQDataEntityModel.Object);
            var testChangeSet = new ChangeSetEntry()
            {
                Operation = ChangeSetOperation.Insert,
                NewValues = new List<KeyValuePair<string, object>>()
            };

            changeSetApplier.ApplyChangeSet(testChangeSet);
            var appliedEntity = changeSetApplier.AppliedChangeSetEntries[testChangeSet];

            mockChangeSetContext.Verify(x => x.AddNewEntity(appliedEntity), Times.Once());
        }

        [Fact]
        public void Apply_Insert_WithExistingEntity_ThrowsChangeSetException()
        {
            var stubChangeSetContext = new Mock<IChangeSetContext>();
            var stubQDataEntityModel = new Mock<IQDataEntityModel>();

            var changeSetApplier = new ChangeSetApplier<TestEntity>(stubChangeSetContext.Object, stubQDataEntityModel.Object);
            var testChangeSet = new ChangeSetEntry()
            {
                Operation = ChangeSetOperation.Insert,
            };

            void applyChangeSet() => changeSetApplier.ApplyChangeSet(testChangeSet);

            Assert.Throws<ChangeSetInvalidException>(applyChangeSet);
        }

        [Fact]
        public void Apply_Insert_WithoutNewValues_ThrowsChangeSetInvalidException()
        {
            var stubChangeSetContext = new Mock<IChangeSetContext>();
            var stubQDataEntityModel = new Mock<IQDataEntityModel>();

            var changeSetApplier = new ChangeSetApplier<TestEntity>(stubChangeSetContext.Object, stubQDataEntityModel.Object);
            var testChangeSet = new ChangeSetEntry()
            {
                Operation = ChangeSetOperation.Insert,
            };

            void applyChangeSet() => changeSetApplier.ApplyChangeSet(testChangeSet);

            Assert.Throws<ChangeSetInvalidException>(applyChangeSet);
        }

        [Fact]
        public void Apply_Insert_WithOldValues_ThrowsChangeSetInvalidException()
        {
            var stubChangeSetContext = new Mock<IChangeSetContext>();
            var stubQDataEntityModel = new Mock<IQDataEntityModel>();

            var changeSetApplier = new ChangeSetApplier<TestEntity>(stubChangeSetContext.Object, stubQDataEntityModel.Object);
            var testChangeSet = new ChangeSetEntry()
            {
                Operation = ChangeSetOperation.Insert,
                OldValues = new List<KeyValuePair<string, object>>()
            };

            void applyChangeSet() => changeSetApplier.ApplyChangeSet(testChangeSet);

            Assert.Throws<ChangeSetInvalidException>(applyChangeSet);
        }
    }
}