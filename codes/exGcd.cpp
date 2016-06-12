#include <iostream>

using namespace std;


bool exGcd(int a, int b, int c, int &x, int& y, int t = 0);  // ax + by = c, a >= b >= 0, t is an any integer

int main(int argc, char** argv)
{
    int a = 11, b = 9, c = 9;
    int x = 0, y = 0;

    cout << a << "x + " << b << "y = " << c << endl;

    if (exGcd(a, b, c, x, y))
    {
        cout << "x=" << x << ", y=" << y << endl;
    }
    else
    {
        cout << "no solution!\n";
    }

    return 0;
}


// a >= b > 0
int gcd(int a, int b)
{
    while (b > 0)
    {
        auto c = a % b;
        a = b;
        b = c;
    }

    return a;
}

// ax + by = gcd(a, b), a >= b >= 0
void exGcd(int a, int b, int& x, int& y)
{
    if (b == 0)
    {
        x = 1;
        y = 0;
        return;
    }

    exGcd(b, a % b, x, y);
    auto z = x;
    x = y;
    y = z - a / b * y;
}

// ax + by = c, a >= b >= 0
bool exGcd(int a, int b, int c, int &x, int& y, int t)
{
    auto ab_gcd = gcd(a, b);
    if (c % ab_gcd != 0)
    {
        return false;
    }

    exGcd(a, b, x, y);

    x = (c * x + b * t) / ab_gcd;
    y = (c * y - a * t) / ab_gcd;

    return true;
}