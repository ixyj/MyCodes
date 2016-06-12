#include "vector3D.h"
#include <cmath>


CVector3D::CVector3D()
{
	value[0] = 0.0f;
	value[1] = 0.0f;
	value[2] = 0.0f;
}

CVector3D::CVector3D(double one, double two, double three)
{
	value[0] = one;
	value[1] = two;
	value[2] = three;
}

CVector3D::CVector3D(const CVector3D& vector3D)
{
	value[0] = vector3D[0];
	value[1] = vector3D[1];
	value[2] = vector3D[2];
}

CVector3D::~CVector3D()
{
}

 void CVector3D::SetValue(double value,int dimensional)
{
	if (dimensional >2)
		dimensional = 2;
	this->operator [](dimensional) = value;
}

double CVector3D::GetDotProduct(const CVector3D& vector3D)
{
	return
			value[0] *vector3D[0]
		+ value[1] *vector3D[1]
		+ value[2] *vector3D[2];
}

CVector3D CVector3D::GetCrossProduct(const CVector3D& vector3D)const
{
	return
	CVector3D(value[1]*vector3D[2] - value[2]*vector3D[1],
							value[2]*vector3D[0] - value[0]*vector3D[2],
							value[0]*vector3D[1] - value[1]*vector3D[0]);
}

double CVector3D::GetModule()const
{
	return
		std::sqrt(value[0]*value[0]+value[1]*value[1]+value[2]*value[2]);
}

void CVector3D::Unite()
{
	double unit = GetModule();
	value[0] /= unit;
	value[1] /= unit;
	value[2] /= unit;
}

const CVector3D CVector3D::operator +(const CVector3D& vector3D)
{
	return 
		CVector3D(value[0] + vector3D[0],
								value[1] + vector3D[1],
								value[2] + vector3D[2]);							
}

const CVector3D CVector3D::operator -(const CVector3D& vector3D)
{
	return 
		CVector3D(value[0] - vector3D[0],
								value[1] - vector3D[1],
								value[2] - vector3D[2]);							
}

CVector3D& CVector3D::operator =(const CVector3D& vector3D)
{
	if (&vector3D != this)
	{
		value[0] =vector3D[0];
		value[1] = vector3D[1];
		value[2] = vector3D[2];	
	}
	return *this;
}

bool CVector3D::operator ==(const CVector3D& vector3D)
{
	return 
		(value[0]== vector3D[0]
		&& value[1] == vector3D[1]
		&& value[2] == vector3D[2]);							
}

const CVector3D& CVector3D::operator +=(const CVector3D& vector3D)
{
	value[0] +=vector3D[0];
	value[1] += vector3D[1];
	value[2] += vector3D[2];	
	return *this;
}

const CVector3D& CVector3D::operator -=(const CVector3D& vector3D)
{
	value[0] -=vector3D[0];
	value[1] -= vector3D[1];
	value[2] -= vector3D[2];	
	return *this;
}

double& CVector3D::operator[](int dimensional)
{
	if (dimensional > 2)
		dimensional = 2;
	return value[dimensional];
}

const double& CVector3D::operator[](int dimensional)const
{
		if (dimensional > 2)
		dimensional = 2;
	return value[dimensional];
}

std::ostream& operator<<(std::ostream& os, const CVector3D& vector3D)
{
	os <<vector3D[0]<<"\t"<<vector3D[1]<<"\t"<<vector3D[2]<<"\n";
	return os;
}

