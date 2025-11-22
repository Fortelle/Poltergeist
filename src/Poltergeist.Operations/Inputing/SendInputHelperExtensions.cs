using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Inputing;

public static class SendInputHelperExtensions
{
    public static SendInputHelper AddKey(this SendInputHelper sendInputHelper, VirtualKey key, bool isUp, KeyboardInputMode mode)
    {
        switch (mode)
        {
            case KeyboardInputMode.Scancode when isUp:
                sendInputHelper.AddScancodeUp(key);
                break;
            case KeyboardInputMode.Scancode:
                sendInputHelper.AddScancodeDown(key);
                break;
            case KeyboardInputMode.Virtual when isUp:
                sendInputHelper.AddVkCodeUp(key);
                break;
            case KeyboardInputMode.Virtual:
                sendInputHelper.AddVkCodeDown(key);
                break;
            default:
                throw new NotSupportedException();
        }
        return sendInputHelper;
    }
}
