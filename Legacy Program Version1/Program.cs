using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Davonte' Whitfield 4/14/2020
//Program is in C#


namespace Password_Validator
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("This program is a Password  Validator");
            Console.WriteLine();
            Console.WriteLine("What makes a good password are the following rules:\n");
            Console.WriteLine("-Minimum length is 8 characters.\n" +
                              "-Maximum length is 15 characters.\n" +
                              "-Should contain at least one number.\n" +
                              "-Should contain at least one special character.\n" + 
                              "-Should not contain spaces.");

            string[] specialchar = new string[] { "!", "@", "#", "$", "%", "^", "&", "*", "?", "_", "+" }; //Made array of special chars to compare in the code

            Console.WriteLine();//skipping a line
            Console.WriteLine("Enter a password: ");

            string password;
            password = Console.ReadLine();

            //---------------All of the main BOOLs and INTs that was used for checking
            bool checkloop = false;
            bool NumCheck = false;
            bool LengthCheck = false;
            bool CharacterCheck = false;
            bool SpaceCheck = false;
            int trynumcounter = 0;
            int catchnumcounter = 0;
            int specialCharCounter = 0;
            int nonSpecialCounter = 0;
            //--------------------

            while (checkloop == false) {

                if (password.Length <= 7 || password.Length >= 16) // checking if it's the wrong length
                {
                    checkloop = false;
                    LengthCheck = false;
                    NumCheck = false;
                    CharacterCheck = false; 
                    SpaceCheck = false;

                    Console.Clear();
                    Console.WriteLine("The password that was entered doesn't fit the length required. It needs to be minimum of 8 and maximum of 15 characters. ");
                    Console.WriteLine();
                    Console.WriteLine("Enter a password: ");
                    password = Console.ReadLine();
                }
                else { LengthCheck = true; }
                

                for (int x = 0; x < password.Length; x++){ // "<=" is here because 'catchnumcounter' needs to hit max of the password. that will end the loop.
                   
                        int checknum = 0; //Only here for "TryParse", not used beyond that.
                        if (int.TryParse(char.ToString(password[x]), out checknum) == true)//converting the 'char' to string and to int
                        {
                            trynumcounter++;
                        }
                        else { catchnumcounter++; }
                  
                    if (catchnumcounter == password.Length) {

                        checkloop = false;
                        LengthCheck = false;
                        NumCheck = false;
                        CharacterCheck = false;
                        SpaceCheck = false; 

                        trynumcounter = 0; catchnumcounter = 0;
                        Console.Clear();
                        Console.WriteLine("The password that was entered doesn't have any numbers. Numbers must be added for a strong password. ");
                        Console.WriteLine();
                        Console.WriteLine("Enter a password: ");
                        password = Console.ReadLine();
                        continue;
                    }
                    //if it finds a number, move on and break the loop and reset everything just in case something else down check out
                    if (trynumcounter>=1) { trynumcounter = 0; catchnumcounter = 0; NumCheck = true;  break;  }
                 }


                for (int x = 0; x < specialchar.Length; x++){


                    if (password.Contains(specialchar[x])){ //if the password has any special checks

                        specialCharCounter++;
                    }
                    else { nonSpecialCounter++; }
                    

                    if (nonSpecialCounter == specialchar.Length){
                        checkloop = false;
                        LengthCheck = false;
                        NumCheck = false;
                        CharacterCheck = false;
                        SpaceCheck = false;

                        nonSpecialCounter = 0; specialCharCounter = 0;
                        Console.Clear();
                        Console.WriteLine("The password that was entered doesn't have Special Characters. Special Chracters must be added for a strong password. ");
                        Console.WriteLine("Some are: !, @, #, $, %, ^, &, *, _, +");
                        Console.WriteLine();
                        Console.WriteLine("Enter a password: ");
                        password = Console.ReadLine();
                        continue;
                    }
                    //if it finds a special character, move on and break the loop and reset everything just in case something else down check out
                    if (specialCharCounter >= 1) { nonSpecialCounter = 0; specialCharCounter = 0; CharacterCheck = true; break; }
                }


                for (int x = 0; x < password.Length; x++)
                {


                    if (password.Contains(" "))//checking for spaces
                    {
                        checkloop = false;
                        LengthCheck = false;
                        NumCheck = false;
                        CharacterCheck = false;
                        SpaceCheck = false;

                        Console.Clear();
                        Console.WriteLine("The password that was entered have spaces. There shouldn't be any spaces.");
                        Console.WriteLine();
                        Console.WriteLine("Enter a password: ");
                        password = Console.ReadLine();
                    }
                    else { SpaceCheck = true; break; }
                }
                //if everthing is good, break the while loop
                if (LengthCheck == true && NumCheck == true && CharacterCheck == true && SpaceCheck == true) { checkloop = true;  break; }
            }

            Console.Clear();
            Console.WriteLine("You have created your password.");
            Console.WriteLine("It is: " + password);
            Console.WriteLine();
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
            
        }
    }
}
