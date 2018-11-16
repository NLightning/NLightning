namespace NLightning.Transport
{
    public enum HandshakeState
    {
        Uninitialized,
        Initialized,
        Act1,
        Act2,
        Act3,
        Finished
    }
}