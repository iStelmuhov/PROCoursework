using System;
using System.Threading;

namespace ServiceAssembly
{
    public class ClientAsyncCall<T>
    {
        T Param { get; set; }

        /// <summary> Gets or sets the client. </summary>
        /// <value> The client. </value>
        Client Client { get; set; }

        /// <summary> Gets or sets the service. </summary>
        /// <value> The service. </value>
        GameRoomService Service { get; set; }

        /// <summary> Constructor. </summary>
        /// <remarks> Simon, 30.12.2009. </remarks>
        /// <param name="service"> The service. </param>
        /// <param name="client">  The client. </param>
        /// <param name="param">   The parameter. </param>
        public ClientAsyncCall(GameRoomService service, Client client, T param)
        {
            Service = service;
            Param = param;
            Client = client;
        }

        /// <summary>   
        /// Invokes the client callback. If a timeout exception occurs, 
        /// the client will be removed from clients' list.
        /// </summary>
        /// <remarks> Simon, 30.12.2009. </remarks>
        /// <param name="clientCallback">   The client callback. </param>
        protected void Invoke(Action<T> clientCallback)
        {
            try
            {
                clientCallback?.Invoke(Param);
            }
            catch (TimeoutException)
            {
                // Remove the client and the user
                Service.Disconnect(Client);
            }
        }

        protected void Invoke(object objCallback)
        {
            Invoke(objCallback as Action<T>);
        }

        public void CallOperationAsync(Action<T> clientCallback)
        {
            ParameterizedThreadStart ts = this.Invoke;
            Thread t = new Thread(ts);
            t.Start(clientCallback);
        }
    }
}