#ifndef _CVECTOR3D_H_
#define _CVECTOR3D_H_

#include <iostream>

class CVector3D
{
public:
	CVector3D();
	CVector3D(double one, double two = 0.0f, double three = 0.0f);
	CVector3D(const CVector3D& vector3D);
	virtual ~CVector3D();
	
public:
	virtual double GetDotProduct(const CVector3D& vector3D);
	virtual CVector3D GetCrossProduct(const CVector3D& vector3D)const;
	virtual double GetModule()const;
	virtual void Unite();
	

public:
	virtual void SetValue(double value = 0.0f, int dimensional = 0);

public:
	const CVector3D operator+(const CVector3D& vector3D);
	const CVector3D operator-(const CVector3D& vector3D);
	CVector3D& operator=(const CVector3D& vector3D);
	bool operator==(const CVector3D& vector3D);
	const CVector3D& operator+=(const CVector3D& vector3D);
	const CVector3D& operator-=(const CVector3D& vector3D);
	double& operator[](int dimensional);
	const double& operator[](int dimensional)const;

	friend  std::ostream& operator<<(std::ostream& os, const CVector3D& vector3D);

private:
	double value[3];
};

#endif //_CVECTOR3D_H_