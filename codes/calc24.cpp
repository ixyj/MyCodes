#include <iostream>
#include <string>
#include <list>
#include <algorithm>
#include <sstream>
#include <iterator>
#include <vector>
using namespace std;

struct Node;
double SimpleCalculator(double v1, double v2, char process);
void Calc24(list<Node>& inputs, Node result, vector<string>& res24, bool is2_2 = false);
void Calculating(const Node& node1, const Node& node2, list<Node>& results);
string ToString(const Node& node);

struct Node
{
    Node(double v = 0) :value(v), processor('\0'), node1(nullptr), node2(nullptr){ }  
    Node(const Node* n1, const Node* n2, char proc)
        : value(SimpleCalculator(n1->value, n2->value, proc))
        , processor(proc)
        , node1(n1)
        , node2(n2)
    { }

    double value;
    char processor;
    const Node* node1;
    const Node* node2;
};

int main(int argc, char** argv)
{
    vector<double> inputs;
    copy(istream_iterator<int>(cin), istream_iterator<int>(), back_insert_iterator<vector<double>>(inputs));

    sort(inputs.begin(), inputs.end());
    list<Node> data;
    vector<string> result24;

    do
    {
        for_each(inputs.begin(), inputs.end(), [&data](double dat){data.push_back(Node(dat)); });

        auto n = *data.begin();
        data.erase(data.begin());

        Calc24(data, n, result24);
        Calc24(data, n, result24, true);
        data.clear();
    } while (next_permutation(inputs.begin(), inputs.end()));
      
    for_each(result24.begin(), unique(result24.begin(), result24.end()), [](string& result)
    {
        cout << result.substr(1, result.length() - 2) << "=24\n";
    });

    return 0;
}

void Calc24(list<Node>& inputs, Node result, vector<string>& res24, bool is2_2)
{
    auto it = inputs.begin();
    if (it == inputs.end())
    {
        if (fabs(result.value - 24) < 1e-3)
        {
            res24.push_back(move(ToString(result)));
        }
        return;
    }

    list<Node> results;
    auto first = *it;
    list<Node> newInputs(++it, inputs.end());
    if (is2_2 && it != inputs.end())
    {
        Calculating(first, *it, results);
        *newInputs.begin() = result;
    }
    else
    {
        Calculating(first, result, results);
    }

    for_each(results.begin(), results.end(), [&](const Node& node)
    {     
        Calc24(newInputs, node, res24);
        Calc24(newInputs, node, res24, true);
    });
}

void Calculating(const Node& node1, const Node& node2, list<Node>& results)
{
    results.push_back(Node(&node1, &node2, '+'));
    results.push_back(node1.value >= node2.value ? Node(&node1, &node2, '-') : Node(&node2, &node1, '-'));
    results.push_back(Node(&node1, &node2, '*'));

    if (node1.value > 1e-3)
    {
        results.push_back(Node(&node2, &node1, '/'));
    }
    if (node2.value > 1e-3)
    {
        results.push_back(Node(&node1, &node2, '/'));
    }
}

double SimpleCalculator(double v1, double v2, char process)
{
    switch (process)
    {
    case '+':
        return v1 + v2;
    case '-':
        return v1 - v2;
    case '*':
        return v1 * v2;
    case '/':
        return v1 / v2;
    default: 
        return LONG_MAX;
    }
}

string ToString(const Node& node)
{
    stringstream ss;

    if (node.node1 == nullptr)
    {
        ss << node.value;
    }
    else
    {
        ss << "(" << ToString(*node.node1) << node.processor << ToString(*node.node2) << ")";
    }

    return ss.str();
}