


// bool array for performing bitwise operations 
using ECS;
using System;

namespace bitmask
{
    public class Bitmask
    {
        private bool[] mask;
        private int size;

        public int Count
        {
            get { return size; }
        }
        public bool this[int index]
        {
            get => mask[index];
            set => mask[index] = value;
        }

        public bool this[ComponentType type]
        {
            get => mask[(int)type];
            set => mask[(int)type] = value;
        }

        public Bitmask(int size)
        {
            this.size = size;
            mask = new bool[size];
        }

        public Bitmask(Bitmask b)
        {
            this.size = b.size;
            this.mask = b.mask;
        }


        // bitwise and
        public Bitmask AND(Bitmask maskB)
        {
            if (maskB == null)
            {
                throw new ArgumentNullException("Null argument given for 'AND' operation");
            }
            if (maskB.size != this.size)
            {
                throw new ArgumentException("Cannot perform 'AND' with bitmasks of different sizes");
            }

            Bitmask mask = new Bitmask(maskB.size);

            for (int i = 0; i < maskB.size; i++)
            {
                mask[i] = maskB.mask[i] & this.mask[i];
            }

            return mask;
        }

        // bitwise or
        public Bitmask OR(Bitmask maskB)
        {
            if (maskB == null)
            {
                throw new ArgumentNullException("Null argument given for 'AND' operation");
            }
            if (maskB.size != this.size)
            {
                throw new ArgumentException("Cannot perform 'AND' with bitmasks of different sizes");
            }

            Bitmask mask = new Bitmask(maskB.size);

            for (int i = 0; i < maskB.size; i++)
            {
                mask[i] = maskB.mask[i] | this.mask[i];
            }

            return mask;
        }

        // bitwise not
        public void NOT()
        {
            for (int i = 0; i < this.size; i++)
            {
                this.mask[i] = !this.mask[i];
            }
        }


        public static bool operator true(Bitmask bitmask)
        {
            foreach (bool b in bitmask.mask)
            {
                if (b) { return true; }
            }
            return false;
        }


        public static bool operator false(Bitmask bitmask)
        {
            foreach (bool b in bitmask.mask)
            {
                if (b) { return true; }
            }
            return false;
        }


    }
}



/*




UNIT TEST

mask1 = new Bitmask(8);
mask2 = new Bitmask(8);

Debug.WriteLine("raw");
for (int i = 0; i < mask1.Count; i++)
{
    Debug.WriteLine(mask1[i]);

}

for (int i = 0; i < mask2.Count; i++)
{
    Debug.WriteLine(mask2[i]);
}

mask1[2] = true;
mask2[4] = true;

Debug.WriteLine("index operator");
for (int i = 0; i < mask1.Count; i++)
{
    Debug.WriteLine(mask1[i]);
                
}

for (int i = 0; i < mask2.Count; i++)
{
    Debug.WriteLine(mask2[i]);
}

Bitmask maskAnd = mask1.AND(mask2);
Debug.WriteLine("And");
for (int i = 0; i < mask1.Count; i++)
{
    Debug.WriteLine(maskAnd[i]);

}

Bitmask maskOr = mask1.OR(mask2);
Debug.WriteLine("or");
for (int i = 0; i < mask1.Count; i++)
{
    Debug.WriteLine(maskOr[i]);

}

            
Debug.WriteLine("not");

Bitmask maskNot = new Bitmask(8);
maskNot[0] = true;
maskNot[1] = true;
maskNot[2] = true;

maskNot.NOT();

for (int i = 0; i < mask1.Count; i++)
{
    Debug.WriteLine(maskNot[i]);
}






*/