# IptablesCtl
A library for control iptables rule via libiptc directly
# Tutorial
Rule, Match and Target is basic concepts of this library. Every model contains options collection.
Iptables rule (iptrule) has some options, like protocol and interface, match collection and target.
Iptrule building starts with Rule model
``` csharp
var rule = new RuleBuilder()
    .SetIp4Src("192.168.3.2/23")
    .SetIp4Dst("192.168.3")
    .SetInInterface("eno8")
    .SetOutInterface("eno45", true, true) // inverted option
    .SetProto("tCp")
    .Accept();
```
Adding some matches and target
``` csharp
var tcpMatch = new TcpMatchBuilder()
    .SetSrcPort(200, 300)
    .SetFlags(new[] { "syn", "fin", "ack" }, new[] { "syn" })
    .SetOption(16, true).Build();
var msqrdTarget = new MasqueradeTargetBuilder()
    .SetPorts(200, 300)
    .SetRandom()
    .Build();
var rule = new RuleBuilder()
    .AddMatch(tcpMatch)
    .SetTarget(msqrdTarget)
    .Build();
```
Append rule to Table/Chain
``` csharp
using (var wr = new IptTransaction(Tables.NAT))
{
    wr.AppendRule(Chains.POSTROUTING, rule);
    wr.Commit();
}
```
You can add some rules to table before commit operation calling. Only one commit per IptTransaction awailable.
## Iptrule read operation from Table/Chain
``` csharp
using (var wr = new IO.IptTransaction())
{
    var rules = wr.GetRules(IO.Chains.INPUT);
}
```
All models stores its options as dictionary. Options key/value format is like to Iptables style
``` csharp
var tcpMatch = new TcpMatchBuilder().SetSrcPort(200, 300)
    .SetFlags(new[] { "syn", "fin", "ack" }, new[] { "syn" })
    .SetOption(16, true)
    .Build();
var optValue = match[TcpMatchBuilder.SPORT_OPT]
System.Console.WriteLine($"{TcpMatchBuilder.SPORT_OPT} {optValue}");
...
# --sport 200:300
```
Rule, Match and Target model has serialization to json and iptrule text format
``` csharp
var tcpMatch = new TcpMatchBuilder()
    .SetSrcPort(1000, 1000)
    .SetDstPort(1002, 1002)
    .Build();
var rule = new RuleBuilder()
    .AddMatch(tcpMatch)
    .Accept();
System.Console.WriteLine(rule);
var ruleJson = System.Text.Json.JsonSerializer.Serialize(rule);
System.Console.WriteLine(ruleJson);
...
# -p tcp --sport 1000 --dport 1002 -j ACCEPT
# {"Matches":[{"Name":"tcp","NeedKey":false,"Revision":0,"Options":{"--sport":"1000","--dport":"1002"}}],"Target":{"Name":"ACCEPT","Revision":0},"Options":{"-p":"tcp"}}
```
## Extention
If some match or target types isn`t represented in basic collection, it can be added yourself. Example with CommentMatch locate at project code.
All struct can be found at [netfilter](http://charette.no-ip.com:81/programming/doxygen/netfilter/index.html) source code.
## Dockerfile
IptablesCtl tested with Ubuntu 18.04 and also can be include to dockerized net.core project. 
``` dockerfile
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
RUN apt update && apt install iptables -y
```
Add capabilites --cap-add NET_ADMIN to run command and expose port range for tcp/udp rules
