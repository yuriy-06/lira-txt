using System;
using  System.Collections.Generic;
using System.Text.RegularExpressions;

public struct Elem  // объявляется структура элементов
{
	public string type; 
	public int node1, node2, node3, node4;
	public float z1, z2, z3, z4, dz1, dz2, dz3, dz4;
	public bool bz1, bz2, bz3, bz4;
	public Elem (string p1, int p2, int p3, int p4, int p5)
	{
		this.type = p1; this.node1 = p2; this.node2 = p3; this.node3 = p4; this.node4 = p5;
		this.z1 = 0f; this.z2 = 0f; this.z3 = 0f; this.z4 = 0f; 
		this.dz1 = 0f; this.dz2 = 0f; this.dz3 = 0f; this.dz4 = 0f;
		this.bz1 = false; this.bz2 = false; this.bz3 = false; this.bz4 = false; 
				
	}
}
public struct Node // объявляется структура узлов
{
	public int number;
	public float x, y, z;
	public Node (int p1, float p2, float p3, float p4)
	{
		this.number = p1; this.x = p2; this.y = p3; this.z = p4;
	}
}
public static class Geom
{
	// возвращает координату Z выбранного узла
	static public float Node_select (int number, Node[] m_node)
	{
		int index = m_node.Length;
		for (int i = 0; i<index; i++)
		{
			if (i == number - 1) { return m_node[i].z;};
		}
		return 0f;
	}
	// 
	static public void Node_edit (int number, Node[] m_node, float z)
	{
		int index = m_node.Length;
		for (int i = 0; i<index; i++)
		{
			if (i == number - 1) { m_node[i].z = z;}; // поменяли координату
		}
	}
}
public static class Mathem
{
	static public float Max (float n1, float n2, float n3, float n4)
	{
		return Math.Max(Math.Max(n1,n2), Math.Max(n3,n4));
	}
	static public float Min (float n1, float n2, float n3, float n4)
	{
		return Math.Min(Math.Min(n1,n2), Math.Min(n3,n4));
	}
}

class Programm
{
    static void Main()
    {
		// начинаем с чтения файла

        string text = System.IO.File.ReadAllText(@"C:\Users\Public\Documents\LIRA SAPR\LIRA SAPR 2013\Data\PF9-lira2.txt");

        System.Console.WriteLine("Начинаем чтение");
		// конструируем регулярное выражение выбирающее весь документ 1 (КЕ) // тестировалось на лире 2013
        const string pat_elem = @"\( 1\/{1}\r{1}\n{1}(((32|41|42|44){1}\s{1}(\d+\s{1})+\/{1})+\r{1}\n{1})+\s{1}\){1}";
		// документ 4 (узлы)
		const string pat_nodes = @"\( 4\/{1}\r{1}\n{1}((((-)?\d*\.?\d+\s{1}){3}\/{1})+\r{1}\n{1})+\s{1}\){1}";
        System.Console.WriteLine("Сконструировали паттерны");
        Regex r_elem = new Regex(pat_elem, RegexOptions.Multiline);
		Regex r_nodes = new Regex(pat_nodes, RegexOptions.Multiline);
        DateTime ThToday1 = DateTime.Now;
        System.Console.WriteLine(ThToday1);
        System.Console.WriteLine("Начинаем поиск вхождения");

        Match match_elem = r_elem.Match(text);
		Match match_nodes = r_nodes.Match(text);
		
        DateTime ThToday2 = DateTime.Now;
        System.Console.WriteLine(ThToday2);
        System.Console.WriteLine("Выбираем группу");
        Group g_elem = match_elem.Groups[0];
		Group g_nodes = match_nodes.Groups[0];
        System.Console.WriteLine("Начинаем запись");
        using (var file = 
               new System.IO.StreamWriter(@"elems.txt"))
        {
        			file.WriteLine(g_elem);
        }
		using (var file = 
               new System.IO.StreamWriter(@"nodes.txt"))
        {
        			file.WriteLine(g_nodes);
        }
		
        //записываем массив объектов элементы
        string elems = System.IO.File.ReadAllText(@"elems.txt"); string nodes = System.IO.File.ReadAllText(@"nodes.txt");
        elems = elems.Replace("\r",""); nodes = nodes.Replace("\r","");
        elems = elems.Replace("\n",""); nodes = nodes.Replace("\n","");
        
        string[] elems_m = elems.Split('/');
		string[] nodes_m = nodes.Split('/');
		
        Array.Clear(elems_m, 0, 1);
		Array.Clear(nodes_m, 0, 1);
		
        int LastIndex_e = elems_m.Length - 1;
		int LastIndex_n = nodes_m.Length - 1;
		
        Array.Clear(elems_m, LastIndex_e, 1);
		Array.Clear(nodes_m, LastIndex_n, 1);
        var m_elem = new Elem[elems_m.Length - 2]; string[] el;
		var m_node = new Node[nodes_m.Length - 2]; string[] nd;
		// заполняем массив структур элементов 
        int j = 0;
        for (int i = 1; i < LastIndex_e; i++)
        {
        	el = elems_m[i].Split(' ');
        	if (el[0] == "32")
        	{
        		m_elem[j] = new Elem(el[0], Int32.Parse(el[2]), Int32.Parse(el[3]), Int32.Parse(el[4]), Int32.Parse(el[5]));
				j++;
        	}        	
        }
        
        
		// заполняем массив структур узлов
		j = 1;
		for (int i = 1; i < LastIndex_n - 1; i++)
        {
        	nd = nodes_m[i].Split(' ');
        	m_node[j] = new Node(j, Single.Parse(nd[0]), Single.Parse(nd[1]), Single.Parse(nd[2]));
        	j++;
        }
		// теперь мы должны определить какие элементы пересекаются гориз. плоскостью плиты
		// для этого итерируем массив структур элементов, для каждого элемента находим максимальную и минимальную координату Z узлов
		// если между ними распологается искомая координата плиты, то мы должны притянуть к плоскости плиты 3 ближайших узла (изменив их координату Z на координату плиты)
		int index = m_elem.Length;
		int[] m1 = {0, 0, 0, 0};
		float[] m2 = {0f, 0f, 0f, 0f};
		//var hesh_nodes = new SortedDictionary<int, float>();
		const float z = 0f;
		float max; float min;
		for (int i = 0; i < index; i++)
		{
			//hesh_nodes.Clear();
			m1[0] = m_elem[i].node1; // выбрали узлы елемента
			m1[1] = m_elem[i].node2;
			m1[2] = m_elem[i].node3;
			m1[3] = m_elem[i].node4;
			m2[0] = Geom.Node_select(m1[0], m_node); // выбрали узлы елемента
			m2[1] = Geom.Node_select(m1[1], m_node);
			m2[2] = Geom.Node_select(m1[2], m_node);
			m2[3] = Geom.Node_select(m1[3], m_node);
			m_elem[i].z1 = m2[0]; m_elem[i].z2 = m2[1]; m_elem[i].z3 = m2[2]; m_elem[i].z4 = m2[3]; 
			if (i == 50000) {Console.WriteLine("50000");};
			//if (i == 200000) {Console.WriteLine("200000");};
			max = Mathem.Max(m2[0], m2[1], m2[2], m2[3]);
			min = Mathem.Min(m2[0], m2[1], m2[2], m2[3]);
			if ((min <= z)&(z <= max))
			{
				// наполняем наш словарь значениями
				//hesh_nodes.Add(m1[0], m2[0]);
				//hesh_nodes.Add(m1[1], m2[1]);
				//hesh_nodes.Add(m1[2], m2[2]);
				//hesh_nodes.Add(m1[3], m2[3]);
				m_elem[i].dz1 = z - m_elem[i].z1; 
				m_elem[i].dz2 = z - m_elem[i].z2; 
				m_elem[i].dz3 = z - m_elem[i].z3; 
				m_elem[i].dz4 = z - m_elem[i].z4; 
				//находим 3 ближайших узла к плите
				float max_dz = Mathem.Max(Math.Abs(m_elem[i].dz1), Math.Abs(m_elem[i].dz2), Math.Abs(m_elem[i].dz3), Math.Abs(m_elem[i].dz4));
				m_elem[i].bz1 = true; m_elem[i].bz2 = true; m_elem[i].bz3 = true; m_elem[i].bz4 = true; 
				if (Math.Abs(m_elem[i].dz1) - max_dz < 0.001) 
				{
					m_elem[i].bz1 = false;
					Geom.Node_edit(m_elem[i].node2, m_node, z);
					Geom.Node_edit(m_elem[i].node3, m_node, z);
					Geom.Node_edit(m_elem[i].node4, m_node, z);
					
				};
				if (Math.Abs(m_elem[i].dz2) - max_dz < 0.001) 
				{
					m_elem[i].bz2 = false;
					Geom.Node_edit(m_elem[i].node1, m_node, z);
					Geom.Node_edit(m_elem[i].node3, m_node, z);
					Geom.Node_edit(m_elem[i].node4, m_node, z);
				};
				if (Math.Abs(m_elem[i].dz3) - max_dz < 0.001) 
				{
					m_elem[i].bz3 = false;
					Geom.Node_edit(m_elem[i].node1, m_node, z);
					Geom.Node_edit(m_elem[i].node2, m_node, z);
					Geom.Node_edit(m_elem[i].node4, m_node, z);
				};
				if (Math.Abs(m_elem[i].dz4) - max_dz < 0.001) 
				{
					m_elem[i].bz4 = false;
					Geom.Node_edit(m_elem[i].node1, m_node, z);
					Geom.Node_edit(m_elem[i].node2, m_node, z);
					Geom.Node_edit(m_elem[i].node3, m_node, z);
				};
				
			};
		}
		// здесь мы должны напрячься и сгенерировать документ 4
		using (var file3 = 
               new System.IO.StreamWriter(@"nodes_edit.txt"))
        {
        	file3.WriteLine("( 4");
			int indexn = m_node.Length;
			for (int i = 0; i<indexn; i++)
			{
				file3.WriteLine(Convert.ToString(m_node[i].x) + " " + Convert.ToString(m_node[i].y) + " " + Convert.ToString(m_node[i].z) + " /");
			}
        	file3.WriteLine(@" )");
        }
		
        // проверка для отладки
        Console.WriteLine(elems_m[LastIndex_e - 1]);
        // блок заканчивающий работу программы
        Console.WriteLine("Press any key to exit.");
        System.Console.ReadKey();
		}
}