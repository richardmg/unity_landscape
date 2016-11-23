
inline float if_eq(float x, float y)
{
	return 1.0 - abs(sign(x - y));
}

inline float if_neq(float x, float y)
{
	return abs(sign(x - y));
}

inline float if_gt(float x, float y)
{
	return max(sign(x - y), 0.0);
}

inline float if_lt(float x, float y)
{
	return max(sign(y - x), 0.0);
}

inline float if_then(float testValue, float thenExpr)
{
	return if_neq(testValue, 0) * thenExpr;
}

inline float if_else(float testValue, float ifExpr, float elseExpr)
{
	return elseExpr + if_then(testValue, ifExpr - elseExpr);
}
