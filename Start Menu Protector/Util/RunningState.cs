namespace StartMenuProtector.Util
{
    public class RunningState 
    {
        public enum Value
        {
            On,
            Off
        }
        
        public Value State { get; }
        
        public static RunningState Enabled  { get; } = new RunningState(state: Value.On);
        public static RunningState Disabled { get; } = new RunningState(state: Value.Off);

        private RunningState(Value state)
        {
            this.State = state;
        }
    }
}