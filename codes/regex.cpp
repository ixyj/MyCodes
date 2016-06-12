#include <iostream>
#include <string>
#include <regex>

using namespace std;

int main(int argc, char** argv)
{
    string testStr("16qw3eyr@23sina.com");
    regex testRegex("\\d+");

    // match针对整个字符串，若整个串匹配，match返回true，否则false
    // search非针对整串，若任意部分子串匹配，search返回true，否则false
    cmatch result;
    if (regex_search(testStr.c_str(), result, testRegex))
    {
        cout << result.str() << endl;

        // print all matches
        regex_iterator<std::string::iterator> it(testStr.begin(), testStr.end(), testRegex);
        regex_iterator<string::iterator> end;
        while (it != end)
        {
            std::cout << it->str() << std::endl;
            ++it;
        }
    }
    else
    {
        cout << "fail to search the regex pattern!" << endl;
    }

    if (regex_match(testStr.c_str(), result, testRegex))
    {
        for (auto ret : result)
        {
            cout << ret.str() << endl;
        }
    }
    else
    {
        cout << "fail to match the regex pattern!" << endl;
    }

    auto replaced = regex_replace(testStr, testRegex, "@#@");
    cout << replaced.c_str() << endl;

    return 0;
}