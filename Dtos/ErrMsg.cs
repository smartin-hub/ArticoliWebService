namespace Dtos {

       public class ErrMsg
    {

        public ErrMsg(string messaggio, string errore)
        {
            this.messaggio = messaggio;
            this.errore = errore;
        }

        public string messaggio { get; set; }
        public string errore { get; set; }
        
        
        
        
    }
}