using System;
namespace IptablesCtl.Native
{
    public enum RejectWith
    {
        IPT_ICMP_NET_UNREACHABLE, 
        IPT_ICMP_HOST_UNREACHABLE,
        IPT_ICMP_PROT_UNREACHABLE,
        IPT_ICMP_PORT_UNREACHABLE,
        IPT_ICMP_ECHOREPLY,
        IPT_ICMP_NET_PROHIBITED,
        IPT_ICMP_HOST_PROHIBITED,
        IPT_TCP_RESET,
        IPT_ICMP_ADMIN_PROHIBITED
    }
};