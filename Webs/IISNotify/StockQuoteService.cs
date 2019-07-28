using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace IISNotify
{
    [ServiceContract]
    public interface IStockQuoteCallback
    {
        [OperationContract(IsOneWay = true)]
        Task SendQuote(string code, double value); 
    }

    [ServiceContract(CallbackContract = typeof(IStockQuoteCallback))]
    public interface IStockQuoteService
    {
        [OperationContract(IsOneWay = true)]
        Task StartSendingQuotes();
    }

    public class StockQuoteService : IStockQuoteService
    {
        public async Task StartSendingQuotes()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IStockQuoteCallback>();
            var random = new Random();
            double price = 29.00;

            while (((IChannel)callback).State == CommunicationState.Opened)
            {
                await callback.SendQuote("MSFT", price);
                price += random.NextDouble();
                await Task.Delay(1000);
            }
        }
    }
}