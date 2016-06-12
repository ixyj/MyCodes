
#include <iostream>

template<class T>
 struct Data
{
	T data;
	Data<T>* next;
	Data<T>* prior;

	Data<T>(T dat){ data = dat;next = prior = NULL;}
};


template<class T>
class SimpleStack
{
	///////////////////////////////////////////
public:
	SimpleStack();
	virtual ~SimpleStack();

public:
	virtual T Pop();
	virtual void Push(T data);
	virtual int Size();
//	bool IsFull();
	virtual bool IsEmpty();
	virtual void Print(int address = -1, std::ostream& fout = cout) const;

	///////////////////////////////////////////
private:
	Data<T>* top;
	Data<T>* head;
	int m_size;
};



template<class T>
SimpleStack<T>::SimpleStack()
:m_size(0)
{
	head = NULL;
	top = NULL;
}

template<class T>
SimpleStack<T>::~SimpleStack()
{
	Data<T>* temp = top;
	while(temp != NULL)
	{
		delete temp;
	    if (top == head)
			break;
		top = top->prior;
		temp = top;
	}
}

template<class T>
void SimpleStack<T>::Push(T data)
{
	Data<T>* temp = new Data<T>(data);
	if (m_size == 0)
	{
		top = temp;
		head = temp;
		head->prior = NULL;
		top->next = NULL;
	}
	else
	{
		top->next = temp;
		temp->prior = top;
		top = top->next;
		top->next = NULL;
	}
	++m_size;
}

template<class T>
T SimpleStack<T>::Pop()
{
	if (m_size <= 0)
	{
		return NULL;
	}
	else
	{
		T data = top->data;
		Data<T> *pData = top;
		if (m_size == 1)
		{
			top = NULL;
			head = NULL;
		}
		else
		{
		top = top->prior;
		top->next = NULL;
		}

		delete pData;
		--m_size;
		return data;
	}
}

//bool SimpleStack<T>::IsFull()
//{
//	return 
//}

template<class T>
bool SimpleStack<T>::IsEmpty()
{
	return m_size == 0;
}

template<class T>
void SimpleStack<T>::Print(int address, std::ostream& fout)const
{
	Data<T>* pData = top;
	if (address == -1)
	{
		for (;pData->prior != NULL; pData = pData->prior)
			fout<<pData->data<<std::ends;
	}
	else if (address > 0 && address <= m_size)
	{
		for (int i = 0; i != address; pData = pData->prior, i++);
		fout<<pData->data<<std::ends;
	}
	else
		fout << "ERROR!\n";
}

template<class T>
int SimpleStack<T>::Size()
{
	return m_size;
}


using namespace std;


int main()
{
	
	SimpleStack<float> s;
	s.Push(12.4f);
	s.Push(3.14f);
	s.Print();
	cout<<s.IsEmpty()<<endl;
	cout<<s.Size()<<endl;
	cout<<s.Pop()<<endl;
	cout<<s.Pop()<<endl;
	cout<<s.Pop()<<endl;
	cout<<s.IsEmpty()<<endl;
	s.Push(0.618f);
	s.Print();
	cout<<s.Size()<<endl;
	return 0;
}