// Test harness for CIDR exclusion logic in SnaffCore.NetworkUtils.
// Compile: csc /reference:SnaffCore.dll Tests\CidrTest.cs
// Run:     CidrTest.exe
using System;
using System.Net;
using SnaffCore;

class CidrTest
{
    static int passed = 0;
    static int failed = 0;

    static void Main()
    {
        // --- IPv4 standard cases ---
        Test("10.1.2.100",   "10.1.2.0/24",    true,  "IPv4 host inside /24");
        Test("10.1.3.1",     "10.1.2.0/24",    false, "IPv4 host outside /24");
        Test("10.5.6.7",     "10.0.0.0/8",     true,  "IPv4 host inside /8");
        Test("11.0.0.1",     "10.0.0.0/8",     false, "IPv4 host outside /8");
        Test("172.16.5.5",   "172.16.0.0/16",  true,  "IPv4 host inside /16");
        Test("172.17.0.1",   "172.16.0.0/16",  false, "IPv4 host outside /16");

        // --- Boundary cases ---
        Test("10.1.2.0",     "10.1.2.0/24",    true,  "Network address itself");
        Test("10.1.2.255",   "10.1.2.0/24",    true,  "Broadcast address");
        Test("10.1.3.0",     "10.1.2.0/24",    false, "First address of next network");

        // --- Non-byte-boundary prefix (/20, /25 etc.) ---
        Test("10.1.16.1",    "10.1.16.0/20",   true,  "IPv4 host inside /20");
        Test("10.1.32.1",    "10.1.16.0/20",   false, "IPv4 host outside /20");
        Test("192.168.1.1",  "192.168.1.0/25", true,  "IPv4 host inside /25 lower half");
        Test("192.168.1.128","192.168.1.0/25", false, "IPv4 host in upper half of /25");

        // --- /32 single host ---
        Test("192.168.1.1",  "192.168.1.1/32", true,  "/32 exact match");
        Test("192.168.1.2",  "192.168.1.1/32", false, "/32 non-match");

        // --- /0 match everything ---
        Test("1.2.3.4",      "0.0.0.0/0",      true,  "IPv4 /0 matches any address");
        Test("255.255.255.255","0.0.0.0/0",     true,  "IPv4 /0 matches broadcast");

        // --- IPv6 ---
        Test("2001:db8::1",  "2001:db8::/32",  true,  "IPv6 host inside /32");
        Test("2001:db9::1",  "2001:db8::/32",  false, "IPv6 host outside /32");
        Test("fe80::1",      "fe80::/10",       true,  "IPv6 link-local inside /10");
        Test("fe40::1",      "fe80::/10",       false, "IPv6 outside link-local /10");

        // --- Mixed family (should never match) ---
        Test("10.1.2.1",     "2001:db8::/32",  false, "IPv4 address vs IPv6 CIDR");
        Test("2001:db8::1",  "10.1.2.0/24",    false, "IPv6 address vs IPv4 CIDR");

        // --- Malformed input (should not throw, just return false) ---
        TestMalformed("notacidr",         "Malformed: no slash");
        TestMalformed("10.1.2.0/abc",     "Malformed: non-numeric prefix");
        TestMalformed("999.999.999.999/24","Malformed: invalid network address");
        TestMalformed("/24",              "Malformed: missing network address");

        Console.WriteLine();
        Console.WriteLine($"Results: {passed} passed, {failed} failed");
        Environment.Exit(failed > 0 ? 1 : 0);
    }

    static void Test(string ip, string cidr, bool expected, string label)
    {
        IPAddress address = IPAddress.Parse(ip);
        bool result = NetworkUtils.IsInCidr(address, cidr);
        bool ok = result == expected;
        if (ok) passed++; else failed++;
        Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {label}: {ip} in {cidr} => {result} (expected {expected})");
    }

    static void TestMalformed(string cidr, string label)
    {
        try
        {
            bool result = NetworkUtils.IsInCidr(IPAddress.Parse("10.1.2.1"), cidr);
            bool ok = result == false;
            if (ok) passed++; else failed++;
            Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {label}: returned {result} (expected false)");
        }
        catch (Exception ex)
        {
            failed++;
            Console.WriteLine($"[FAIL] {label}: threw exception: {ex.Message}");
        }
    }
}
