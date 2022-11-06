namespace Polyperfect.Crafting.Integration.Tests
{
    public class UGUITests
    {
        /*[Test]
        public IEnumerator ItemTransfer()
        {
            EnsureEventsExist();
            yield return new WaitForSeconds(1f);
            CreateSlotObject(out var sourceRect, out var sourceSlot);
            CreateSlotObject(out var destRect, out var destSlot);
            var transferObject = CreateSlotObject(out var transferRect, out var transferSlot);

            var transfer = transferObject.AddComponent<UGUIItemTransfer>();

            var item = new ItemStack(RuntimeID.Random(),10);
            sourceSlot.InsertCompletely(item);

            sourceRect.position = new Vector3(50, 50);
            destRect.position = new Vector3(500, 200);

            var arr = new Vector3[4];
            sourceRect.GetWorldCorners(arr);
            Assert.IsTrue(transfer.TransferAtPosition(new UGUIItemTransfer.ScreenTransferArgs(GetAverage(arr),UGUIItemTransfer.ScreenTransferArgs.TransferType.All)));
            destRect.GetWorldCorners(arr);
            Assert.IsTrue(transfer.TransferAtPosition(new UGUIItemTransfer.ScreenTransferArgs(GetAverage(arr),UGUIItemTransfer.ScreenTransferArgs.TransferType.All)));


            Assert.IsTrue(sourceSlot.Peek().IsEmpty());
            Assert.IsTrue(transferSlot.Peek().IsEmpty());
            Assert.IsFalse(destSlot.Peek().IsEmpty());
        }

        void EnsureEventsExist()
        {
            if (!EventSystem.current)
                EventSystem.current = new GameObject().AddComponent<EventSystem>();
        }

        GameObject CreateSlotObject(out RectTransform rect, out ItemSlotComponent itemSlot)
        {
            var go = new GameObject();
            rect = go.AddComponent<RectTransform>();
            itemSlot = go.AddComponent<ItemSlotComponent>();
            go.AddComponent<UGUIInvisibleRaycastTarget>();
            return go;
        }

        Vector3 GetAverage(Vector3[] pos)
        {
            if (pos.Length <= 0f)
                return Vector3.zero;
                
            var total = Vector3.zero;
            foreach (var t in pos) 
                total += t;

            return total / pos.Length;
        }*/
    }
}