#include <iostream>
#include <string>
#include <vector>
#include <queue> 
#include <algorithm>
using namespace std;

template<class T> struct Node;
template<class T> bool GetTreeFromPreInOrder(vector<T> preOrder, vector<T> inOrder, Node<T>*& root);
template<class T> void BFT(Node<T>* root);
template<class T> void FreeTree(Node<T>*& root);


int main(int argc, char** argv)
{
    vector<string> pre = { "A", "B", "D", "G", "H", "E", "C", "K", "F", "I", "J" };
    vector<string> in = { "G", "D", "H", "B", "E", "A", "K", "C", "I", "J", "F" };

    Node<string>* root = nullptr;

    GetTreeFromPreInOrder(pre, in, root);

    BFT(root);

    FreeTree(root);

    return 0;
}

template<class T>
struct Node
{
    T value;
    Node* left;
    Node* right;
};

template<class T>
bool GetTreeFromPreInOrder(vector<T> preOrder, vector<T> inOrder, Node<T>*& root)
{
    if (preOrder.size() != inOrder.size())
    {
        return false;
    }

    if (preOrder.empty())
    {
        root = nullptr;
        return true;
    }

    auto rootIn = find(inOrder.begin(), inOrder.end(), *preOrder.begin());
    if (rootIn == inOrder.end())
    {
        return false;
    }

    auto inLeftSize = rootIn - inOrder.begin();
    vector<T> left_pre(preOrder.begin() + 1, preOrder.begin() + inLeftSize + 1), left_in(inOrder.begin(), rootIn);
    vector<T> right_pre(preOrder.begin() + inLeftSize + 1, preOrder.end()), right_in(rootIn + 1, inOrder.end());

    root = new Node<T>();
    root->value = *rootIn;
    return  GetTreeFromPreInOrder(left_pre, left_in, root->left) && GetTreeFromPreInOrder(right_pre, right_in, root->right);

}

// Note that null node will have no any sons
template<class T>
void BFT(Node<T>* root)
{
    queue<Node<T>*> nodes;
    nodes.push(root);
    while (!nodes.empty())
    {
        auto node = nodes.front();
        nodes.pop();
        if (node != nullptr)
        {
            cout << node->value << " ";
            nodes.push(node->left);
            nodes.push(node->right);
        }
        else
        {
            cout << "? ";
        }
    }
}

template<class T>
void FreeTree(Node<T>*& root)
{
    if (root == nullptr)
    {
        return;
    }

    FreeTree(root->left);
    FreeTree(root->right);

    delete root;
    root = nullptr;
}
