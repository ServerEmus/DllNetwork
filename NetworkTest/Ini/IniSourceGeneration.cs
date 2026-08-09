using DllNetwork.Settings;
using IniParser;

namespace NetworkTest.Ini;

[GenerateIni(typeof(NetworkSettings))]
public partial class IniSourceGeneration;