using Sandbox;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public struct EntityReference<T> : IEqualityComparer<EntityReference<T>>, IValid where T : Entity
{
	public int NetworkIdent { get; private set; }
	public T Entity => Sandbox.Entity.FindByIndex( NetworkIdent ) as T;
	public bool IsValid => Entity.IsValid();

	public EntityReference( T entity )
	{
		NetworkIdent = entity.NetworkIdent;
	}

	public bool Equals( EntityReference<T> b1, EntityReference<T> b2 )
	{
		return b1.NetworkIdent == b2.NetworkIdent;
	}

	public override int GetHashCode()
	{
		return NetworkIdent.GetHashCode();
	}

	public int GetHashCode( [DisallowNull] EntityReference<T> obj )
	{
		return obj.NetworkIdent.GetHashCode();
	}
}
